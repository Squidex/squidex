// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Operations;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentState>
    {
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>(x => x.Wrapping(new DefaultContentWorkflow()));
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
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

        protected override DomainId Id
        {
            get { return DomainId.Combine(AppId, contentId); }
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

            A.CallTo(() => appProvider.GetAppAsync(AppName, false))
                .Returns(app);

            A.CallTo(() => appProvider.GetAppWithSchemaAsync(AppId, SchemaId, false))
                .Returns((app, schema));

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions()))
                .ReturnsLazily(x => Task.FromResult(x.GetArgument<ScriptVars>(0)!.Data!));

            patched = patch.MergeInto(data);

            var validators = Enumerable.Repeat(new DefaultValidatorsFactory(), 1);

            var context = new ContentOperationContext(appProvider,
                validators,
                contentWorkflow,
                contentRepository,
                scriptEngine, A.Fake<ISemanticLog>());

            sut = new ContentDomainObject(Store, A.Dummy<ISemanticLog>(), context);
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
        public async Task Create_should_create_events_and_update_data_and_status()
        {
            var command = new CreateContent { Data = data };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);
            Assert.Same(data, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft), "<create-script>", ScriptOptions()))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_create_events_and_update_status_when_publishing()
        {
            var command = new CreateContent { Data = data, Publish = true };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Published, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft), "<create-script>", ScriptOptions()))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Published), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_throw_when_invalid_data_is_passed()
        {
            var command = new CreateContent { Data = invalidData };

            await Assert.ThrowsAsync<ValidationException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task Upsert_should_create_contnet_when_not_found()
        {
            var command = new UpsertContent { Data = data };

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Same(data, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft), "<create-script>", ScriptOptions()))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_update_contnet_when_found()
        {
            var command = new UpsertContent { Data = otherData };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(otherData, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(otherData, data, Status.Draft), "<update-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_create_events_and_update_data()
        {
            var command = new UpdateContent { Data = otherData };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(otherData, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(otherData, data, Status.Draft), "<update-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_create_events_and_update_new_version_when_draft_available()
        {
            var command = new UpdateContent { Data = otherData };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(otherData, sut.Snapshot.NewVersion?.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(otherData, data, Status.Draft), "<update-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_not_create_event_for_same_data()
        {
            var command = new UpdateContent { Data = data };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Single(LastEvents);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<update-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_when_invalid_data_is_passed()
        {
            var command = new UpdateContent { Data = invalidData };

            await ExecuteCreateAsync();

            await Assert.ThrowsAsync<ValidationException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task Patch_should_create_events_and_update_data()
        {
            var command = new PatchContent { Data = patch };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEqual(data, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = patched })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(patched, data, Status.Draft), "<update-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_create_events_and_update_new_version_when_draft_available()
        {
            var command = new PatchContent { Data = patch };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(patched, sut.Snapshot.NewVersion?.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = patched })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(patched, data, Status.Draft), "<update-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_not_create_event_for_same_data()
        {
            var command = new PatchContent { Data = data };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Single(LastEvents);

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<update-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_when_published()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Published, sut.Snapshot.CurrentVersion.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Published, Change = StatusChange.Published })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Published, Status.Draft), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_when_changed()
        {
            var command = new ChangeContentStatus { Status = Status.Archived };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Archived, sut.Snapshot.CurrentVersion.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_when_unpublished()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Draft, Change = StatusChange.Unpublished })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_also_update_when_script_changes_data()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions()))
                .Returns(otherData);

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);
            Assert.Equal(otherData, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Draft, Change = StatusChange.Unpublished })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_new_version_when_draft_available()
        {
            var command = new ChangeContentStatus { Status = Status.Archived };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Archived, sut.Snapshot.NewVersion?.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Change = StatusChange.Change, Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptContext(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_create_events_and_set_schedule_when_duetime_set()
        {
            var dueTime = Instant.MaxValue;

            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTime };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);
            Assert.Equal(Status.Published, sut.Snapshot.ScheduleJob?.Status);

            Assert.Equal(dueTime, sut.Snapshot.ScheduleJob?.DueTime);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusScheduled { Status = Status.Published, DueTime = dueTime })
                );

            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_refresh_properties_and_unset_schedule_when_completed()
        {
            var dueTime = Instant.MaxValue;

            await ExecuteCreateAsync();
            await ExecuteChangeStatusAsync(Status.Archived, dueTime);

            var command = new ChangeContentStatus { Status = Status.Archived, StatusJobId = sut.Snapshot.ScheduleJob!.Id };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(A<IContentEntity>._, Status.Draft, Status.Archived, User))
                .Returns(true);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.ScheduleJob);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<change-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_unset_schedule_when_failed()
        {
            var dueTime = Instant.MaxValue;

            await ExecuteCreateAsync();
            await ExecuteChangeStatusAsync(Status.Published, dueTime);

            var command = new ChangeContentStatus { Status = Status.Published, StatusJobId = sut.Snapshot.ScheduleJob!.Id };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(A<IContentEntity>._, Status.Draft, Status.Published, User))
                .Returns(false);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.ScheduleJob);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentSchedulingCancelled())
                );

            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions()))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_deleted_flag()
        {
            var command = new DeleteContent();

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );

            A.CallTo(() => scriptEngine.ExecuteAsync(ScriptContext(data, null, Status.Draft), "<delete-script>", ScriptOptions()))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_throw_exception_if_referenced_by_other_item()
        {
            var command = new DeleteContent { CheckReferrers = true };

            await ExecuteCreateAsync();

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task Delete_should_not_throw_exception_if_referenced_by_other_item_but_forced()
        {
            var command = new DeleteContent();

            await ExecuteCreateAsync();

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId))
                .Returns(true);

            await PublishAsync(command);
        }

        [Fact]
        public async Task CreateDraft_should_create_events_and_update_new_state()
        {
            var command = new CreateContentDraft();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Status.Draft, sut.Snapshot.NewVersion?.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDraftCreated { Status = Status.Draft })
                );
        }

        [Fact]
        public async Task DeleteDraft_should_create_events_and_delete_new_version()
        {
            var command = new DeleteContentDraft();

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(new EntitySavedResult(3));

            Assert.Null(sut.Snapshot.NewVersion);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDraftDeleted())
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateContent { Data = data });
        }

        private Task ExecuteUpdateAsync()
        {
            return PublishAsync(new UpdateContent { Data = otherData });
        }

        private Task ExecuteCreateDraftAsync()
        {
            return PublishAsync(new CreateContentDraft());
        }

        private Task ExecuteChangeStatusAsync(Status status, Instant? dueTime = null)
        {
            return PublishAsync(new ChangeContentStatus { Status = status, DueTime = dueTime });
        }

        private Task ExecuteDeleteAsync()
        {
            return PublishAsync(CreateContentCommand(new DeleteContent()));
        }

        private Task ExecutePublishAsync()
        {
            return PublishAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Published }));
        }

        private ScriptVars ScriptContext(NamedContentData? newData, NamedContentData? oldData, Status newStatus)
        {
            return A<ScriptVars>.That.Matches(x => M(x, newData, oldData, newStatus, default));
        }

        private ScriptVars ScriptContext(NamedContentData? newData, NamedContentData? oldData, Status newStatus, Status oldStatus)
        {
            return A<ScriptVars>.That.Matches(x => M(x, newData, oldData, newStatus, oldStatus));
        }

        private static ScriptOptions ScriptOptions()
        {
            return A<ScriptOptions>.That.Matches(x => x.CanDisallow && x.CanReject && x.AsContext);
        }

        private bool M(ScriptVars x, NamedContentData? newData, NamedContentData? oldData, Status newStatus, Status oldStatus)
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
