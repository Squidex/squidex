// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentGrainTests : HandlerTestBase<ContentGrain, ContentState>
    {
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.DE);

        private readonly NamedContentData invalidData =
            new NamedContentData()
                .AddField("my-field1",
                new ContentFieldData()
                        .AddValue(null))
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddValue(1));
        private readonly NamedContentData data =
            new NamedContentData()
                .AddField("my-field1",
                    new ContentFieldData()
                        .AddValue("iv", 1));
        private readonly NamedContentData patch =
            new NamedContentData()
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddValue("iv", 2));
        private readonly NamedContentData otherData =
            new NamedContentData()
                .AddField("my-field1",
                    new ContentFieldData()
                        .AddValue("iv", 2))
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddValue("iv", 2));
        private readonly NamedContentData patched;
        private readonly Guid contentId = Guid.NewGuid();
        private readonly ContentGrain sut;

        protected override Guid Id
        {
            get { return contentId; }
        }

        public ContentGrainTests()
        {
            var schemaDef =
                 new Schema("my-schema")
                     .AddNumber(1, "my-field1", Partitioning.Invariant,
                         new NumberFieldProperties { IsRequired = true })
                     .AddNumber(2, "my-field2", Partitioning.Invariant,
                         new NumberFieldProperties { IsRequired = false });

            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);

            A.CallTo(() => appProvider.GetAppAsync(AppName)).Returns(app);
            A.CallTo(() => appProvider.GetAppWithSchemaAsync(AppId, SchemaId)).Returns((app, schema));

            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
            A.CallTo(() => schema.ScriptCreate).Returns("<create-script>");
            A.CallTo(() => schema.ScriptChange).Returns("<change-script>");
            A.CallTo(() => schema.ScriptUpdate).Returns("<update-script>");
            A.CallTo(() => schema.ScriptDelete).Returns("<delete-script>");

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .ReturnsLazily(x => x.GetArgument<ScriptContext>(0).Data);

            patched = patch.MergeInto(data);

            sut = new ContentGrain(Store, A.Dummy<ISemanticLog>(), appProvider, A.Dummy<IAssetRepository>(), scriptEngine, A.Dummy<IContentRepository>());
            sut.OnActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_content_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_state()
        {
            var command = new CreateContent { Data = data };

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(data, 0));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>"))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_also_publish()
        {
            var command = new CreateContent { Data = data, Publish = true };

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(data, 1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<create-script>"))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_throw_when_invalid_data_is_passed()
        {
            var command = new CreateContent { Data = invalidData };

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(CreateContentCommand(command)));
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new UpdateContent { Data = otherData };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(otherData, 1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_create_proposal_events_and_update_state()
        {
            var command = new UpdateContent { Data = otherData, AsDraft = true };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(otherData, 2));

            Assert.True(sut.Snapshot.IsPending);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdateProposed { Data = otherData })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_not_create_event_for_same_data()
        {
            var command = new UpdateContent { Data = data };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(data, 0));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_when_invalid_data_is_passed()
        {
            var command = new UpdateContent { Data = invalidData };

            await ExecuteCreateAsync();

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(CreateContentCommand(command)));
        }

        [Fact]
        public async Task Patch_should_create_events_and_update_state()
        {
            var command = new PatchContent { Data = patch };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(patched, 1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = patched })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_create_proposal_events_and_update_state()
        {
            var command = new PatchContent { Data = patch, AsDraft = true };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(otherData, 2));

            Assert.True(sut.Snapshot.IsPending);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdateProposed { Data = patched })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_not_create_event_for_same_data()
        {
            var command = new PatchContent { Data = data };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new ContentDataChangedResult(data, 0));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangedStatus_should_create_events_and_update_state()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal(Status.Published, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangedStatus_should_create_events_and_update_state_when_archived()
        {
            var command = new ChangeContentStatus { Status = Status.Archived };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal(Status.Archived, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived, Change = StatusChange.Archived })
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangedStatus_should_create_events_and_update_state_when_unpublished()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Draft, Change = StatusChange.Unpublished })
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangedStatus_should_create_events_and_update_state_when_restored()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            await ExecuteCreateAsync();
            await ExecuteArchiveAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Draft, Change = StatusChange.Restored })
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangedStatus_should_create_proposal_events_and_update_state()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteProposeUpdateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(sut.Snapshot.IsPending);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentChangesPublished())
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<update-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_refresh_properties_and_create_scheduled_events_when_command_has_due_time()
        {
            var dueTime = Instant.MaxValue;

            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTime };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal(Status.Draft, sut.Snapshot.Status);
            Assert.Equal(Status.Published, sut.Snapshot.ScheduleJob.Status);
            Assert.Equal(dueTime, sut.Snapshot.ScheduleJob.DueTime);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusScheduled { Status = Status.Published, DueTime = dueTime })
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_refresh_properties_and_revert_scheduling_when_invoked_by_scheduler()
        {
            await ExecuteCreateAsync();
            await ExecuteScheduledAsync();

            var command = new ChangeContentStatus { Status = Status.Draft, JobId = sut.Snapshot.ScheduleJob.Id };

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.Null(sut.Snapshot.ScheduleJob);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentSchedulingCancelled())
                );
        }

        [Fact]
        public async Task Delete_should_update_properties_and_create_events()
        {
            var command = new DeleteContent();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<delete-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DiscardChanges_should_update_properties_and_create_events()
        {
            var command = new DiscardChanges();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteProposeUpdateAsync();

            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.False(sut.Snapshot.IsPending);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentChangesDiscarded())
                );
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new CreateContent { Data = data }));
        }

        private Task ExecuteUpdateAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new UpdateContent { Data = otherData }));
        }

        private Task ExecuteProposeUpdateAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new UpdateContent { Data = otherData, AsDraft = true }));
        }

        private Task ExecuteScheduledAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Published, DueTime = Instant.MaxValue }));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new DeleteContent()));
        }

        private Task ExecuteArchiveAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Archived }));
        }

        private Task ExecutePublishAsync()
        {
            return sut.ExecuteAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Published }));
        }

        protected T CreateContentEvent<T>(T @event) where T : ContentEvent
        {
            @event.ContentId = contentId;

            return CreateEvent(@event);
        }

        protected T CreateContentCommand<T>(T command) where T : ContentCommand
        {
            command.ContentId = contentId;

            return CreateCommand(command);
        }
    }
}
