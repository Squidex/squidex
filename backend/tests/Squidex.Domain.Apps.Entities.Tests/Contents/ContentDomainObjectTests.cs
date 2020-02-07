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
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentState>
    {
        private readonly Guid contentId = Guid.NewGuid();
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentRepository contentRepository = A.Dummy<IContentRepository>();
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>(x => x.Wrapping(new DefaultContentWorkflow()));
        private readonly ISchemaEntity schema;
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();

        private readonly NamedContentData invalidData =
            new NamedContentData()
                .AddField("my-field1",
                new ContentFieldData()
                        .AddValue("iv", null))
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddValue("iv", 1));
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
        private readonly ContentDomainObject sut;

        protected override Guid Id
        {
            get { return contentId; }
        }

        public ContentDomainObjectTests()
        {
            app = Mocks.App(AppNamedId, Language.DE);

            var scripts = new SchemaScripts
            {
                Change = "<change-script>",
                Create = "<create-script>",
                Delete = "<delete-script>",
                Update = "<update-script>"
            };

            var schemaDef =
                 new Schema("my-schema")
                     .AddNumber(1, "my-field1", Partitioning.Invariant,
                         new NumberFieldProperties { IsRequired = true })
                     .AddNumber(2, "my-field2", Partitioning.Invariant,
                         new NumberFieldProperties { IsRequired = false })
                    .SetScripts(scripts);

            schema = Mocks.Schema(AppNamedId, SchemaNamedId, schemaDef);

            A.CallTo(() => appProvider.GetAppAsync(AppName))
                .Returns(app);

            A.CallTo(() => appProvider.GetAppWithSchemaAsync(AppId, SchemaId))
                .Returns((app, schema));

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, A<string>.Ignored))
                .ReturnsLazily(x => x.GetArgument<ScriptContext>(0)!.Data!);

            patched = patch.MergeInto(data);

            sut = new ContentDomainObject(Store, A.Dummy<ISemanticLog>(), appProvider, A.Dummy<IAssetRepository>(), scriptEngine, contentWorkflow, contentRepository);
            sut.Setup(Id);
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

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(ScriptContext(data, null, Status.Draft), "<create-script>"))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_also_publish()
        {
            var command = new CreateContent { Data = data, Publish = true };

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Published, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(ScriptContext(data, null, Status.Draft), "<create-script>"))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Published), "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_throw_when_invalid_data_is_passed()
        {
            var command = new CreateContent { Data = invalidData };

            await Assert.ThrowsAsync<ValidationException>(() => PublishAsync(CreateContentCommand(command)));
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new UpdateContent { Data = otherData };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(ScriptContext(otherData, data, Status.Draft), "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_not_create_event_for_same_data()
        {
            var command = new UpdateContent { Data = data };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Single(LastEvents);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_when_invalid_data_is_passed()
        {
            var command = new UpdateContent { Data = invalidData };

            await ExecuteCreateAsync();

            await Assert.ThrowsAsync<ValidationException>(() => PublishAsync(CreateContentCommand(command)));
        }

        [Fact]
        public async Task Patch_should_create_events_and_update_state()
        {
            var command = new PatchContent { Data = patch };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = patched })
                );

            A.CallTo(() => scriptEngine.ExecuteAndTransform(ScriptContext(patched, data, Status.Draft), "<update-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_not_create_event_for_same_data()
        {
            var command = new PatchContent { Data = data };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Single(LastEvents);

            A.CallTo(() => scriptEngine.ExecuteAndTransform(A<ScriptContext>.Ignored, "<update-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_state()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Published, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Change = StatusChange.Published, Status = Status.Published })
                );

            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Published, Status.Draft), "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_state_when_archived()
        {
            var command = new ChangeContentStatus { Status = Status.Archived };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Archived, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Archived, Status.Draft), "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_state_when_unpublished()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Change = StatusChange.Unpublished, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Draft, Status.Published), "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_state_when_restored()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            await ExecuteCreateAsync();
            await ExecuteArchiveAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Draft, Status.Archived), "<change-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_proposal_events_and_update_state()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.NewStatus);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
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

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.Status);
            Assert.Equal(Status.Published, sut.Snapshot.ScheduleJob!.Status);
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
            await ExecuteChangeStatusAsync(Status.Published, Instant.MaxValue);

            var command = new ChangeContentStatus { Status = Status.Published, JobId = sut.Snapshot.ScheduleJob!.Id };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(A<IContentInfo>.Ignored, Status.Draft, Status.Published, User))
                .Returns(false);

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.ScheduleJob);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentSchedulingCancelled())
                );

            A.CallTo(() => scriptEngine.Execute(A<ScriptContext>.Ignored, "<change-script>"))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Delete_should_update_properties_and_create_events()
        {
            var command = new DeleteContent();

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );

            A.CallTo(() => scriptEngine.Execute(ScriptContext(data, null, Status.Draft), "<delete-script>"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task CreateDraft_should_create_events_and_update_state()
        {
            var command = new CreateContentDraft();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.NewStatus);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDraftCreated { Status = Status.Draft })
                );
        }

        [Fact]
        public async Task DeleteDraft_should_update_properties_and_create_events()
        {
            var command = new DeleteContentDraft();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.Null(sut.Snapshot.NewStatus);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDraftDeleted())
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(CreateContentCommand(new CreateContent { Data = data }));
        }

        private Task ExecuteUpdateAsync()
        {
            return PublishAsync(CreateContentCommand(new UpdateContent { Data = otherData }));
        }

        private Task ExecuteCreateDraftAsync()
        {
            return PublishAsync(CreateContentCommand(new CreateContentDraft()));
        }

        private Task ExecuteChangeStatusAsync(Status status, Instant? dueTime = null)
        {
            return PublishAsync(CreateContentCommand(new ChangeContentStatus { Status = status, DueTime = dueTime }));
        }

        private Task ExecuteDeleteAsync()
        {
            return PublishAsync(CreateContentCommand(new DeleteContent()));
        }

        private Task ExecuteArchiveAsync()
        {
            return PublishAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Archived }));
        }

        private Task ExecutePublishAsync()
        {
            return PublishAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Published }));
        }

        private ScriptContext ScriptContext(NamedContentData? newData, NamedContentData? oldData, Status newStatus)
        {
            return A<ScriptContext>.That.Matches(x => M(x, newData, oldData, newStatus, default));
        }

        private ScriptContext ScriptContext(NamedContentData? newData, NamedContentData? oldData, Status newStatus, Status oldStatus)
        {
            return A<ScriptContext>.That.Matches(x => M(x, newData, oldData, newStatus, oldStatus));
        }

        private bool M(ScriptContext x, NamedContentData? newData, NamedContentData? oldData, Status newStatus, Status oldStatus)
        {
            return
                Equals(x.Data, newData) &&
                Equals(x.DataOld, oldData) &&
                Equals(x.Status, newStatus) &&
                Equals(x.StatusOld, oldStatus) &&
                x.ContentId == contentId && x.User == User;
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

        private async Task<object?> PublishAsync(ContentCommand command)
        {
            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            return result;
        }
    }
}
