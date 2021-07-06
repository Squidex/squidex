﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentDomainObject.State>
    {
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly IAppEntity app;
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>(x => x.Wrapping(new DefaultContentWorkflow()));
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly ISchemaEntity schema;
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();

        private readonly ContentData invalidData =
            new ContentData()
                .AddField("my-field1",
                new ContentFieldData()
                        .AddInvariant(null))
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddInvariant(1));
        private readonly ContentData data =
            new ContentData()
                .AddField("my-field1",
                    new ContentFieldData()
                        .AddInvariant(1));
        private readonly ContentData patch =
            new ContentData()
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddInvariant(2));
        private readonly ContentData otherData =
            new ContentData()
                .AddField("my-field1",
                    new ContentFieldData()
                        .AddInvariant(2))
                .AddField("my-field2",
                    new ContentFieldData()
                        .AddInvariant(2));
        private readonly ContentData patched;
        private readonly ContentDomainObject sut;

        protected override DomainId Id
        {
            get => DomainId.Combine(AppId, contentId);
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
                 new Schema("my-schema").Publish()
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

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, A<string>._, ScriptOptions(), default))
                .ReturnsLazily(x => Task.FromResult(x.GetArgument<ScriptVars>(0)!.Data!));

            patched = patch.MergeInto(data);

            var log = A.Fake<ISemanticLog>();

            var serviceProvider =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(log)
                    .AddSingleton(contentWorkflow)
                    .AddSingleton(contentRepository)
                    .AddSingleton(scriptEngine)
                    .AddSingleton(TestUtils.DefaultSerializer)
                    .AddSingleton<IValidatorsFactory>(new DefaultValidatorsFactory())
                    .BuildServiceProvider();

            sut = new ContentDomainObject(PersistenceFactory, log, serviceProvider);
            sut.Setup(Id);
        }

        [Fact]
        public async Task Command_should_throw_exception_if_content_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_data_and_status()
        {
            var command = new CreateContent { Data = data };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_not_change_status_if_set_to_initial()
        {
            var command = new CreateContent { Data = data, Status = Status.Draft };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Draft, sut.Snapshot.CurrentVersion.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Create_should_change_status_if_set()
        {
            var command = new CreateContent { Data = data, Status = Status.Archived };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Archived, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Create_should_recreate_deleted_content()
        {
            var command = new CreateContent { Data = data };

            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await PublishAsync(command);
        }

        [Fact]
        public async Task Create_should_recreate_permanently_deleted_content()
        {
            var command = new CreateContent { Data = data };

            await ExecuteCreateAsync();
            await ExecuteDeleteAsync(true);

            await PublishAsync(command);
        }

        [Fact]
        public async Task Create_should_throw_exception_if_invalid_data_is_passed()
        {
            var command = new CreateContent { Data = invalidData };

            await Assert.ThrowsAsync<ValidationException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task Upsert_should_create_content_if_not_found()
        {
            var command = new UpsertContent { Data = data };

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_not_change_status_on_create_if_status_set_to_initial()
        {
            var command = new UpsertContent { Data = data };

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Draft, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_change_status_on_create_if_status_set()
        {
            var command = new UpsertContent { Data = data, Status = Status.Archived };

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(data, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Archived, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_update_content_if_found()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_not_change_status_on_update_if_status_set_to_initial()
        {
            var command = new UpsertContent { Data = otherData, Status = Status.Draft };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(otherData, sut.Snapshot.CurrentVersion.Data);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_change_status_on_update_if_status_set()
        {
            var command = new UpsertContent { Data = otherData, Status = Status.Archived };

            await ExecuteCreateAsync();

            var result = await PublishAsync(CreateContentCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(otherData, sut.Snapshot.CurrentVersion.Data);
            Assert.Equal(Status.Archived, sut.Snapshot.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData }),
                    CreateContentEvent(new ContentStatusChanged { Status = Status.Archived })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), default))
                .MustHaveHappened();
            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Upsert_should_recreate_deleted_content()
        {
            var command = new UpsertContent { Data = data };

            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await PublishAsync(command);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );
        }

        [Fact]
        public async Task Upsert_should_recreate_permanently_deleted_content()
        {
            var command = new UpsertContent { Data = data };

            await ExecuteCreateAsync();
            await ExecuteDeleteAsync(true);

            await PublishAsync(command);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data, Status = Status.Draft })
                );
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Update_should_create_events_and_update_new_version_if_draft_available()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), default))
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

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Update_should_throw_exception_if_invalid_data_is_passed()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(patched, data, Status.Draft), "<update-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Patch_should_create_events_and_update_new_version_if_draft_available()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(patched, data, Status.Draft), "<update-script>", ScriptOptions(), default))
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

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<update-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_if_published()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_if_changed()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_status_if_unpublished()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_also_update_if_script_changes_data()
        {
            var command = new ChangeContentStatus { Status = Status.Draft };

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), default))
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_update_new_version_if_draft_available()
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

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_delete_new_version_if_available()
        {
            var command = new ChangeContentStatus { Status = Status.Published };

            await ExecuteCreateAsync();
            await ExecutePublishAsync();
            await ExecuteCreateDraftAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.NewVersion?.Status);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentStatusChanged { Change = StatusChange.Published, Status = Status.Published })
                );

            A.CallTo(() => scriptEngine.TransformAsync(ScriptVars(data, null, Status.Published, Status.Draft), "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_create_events_and_set_schedule_if_duetime_set()
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

            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_refresh_properties_and_unset_schedule_if_completed()
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

            A.CallTo(() => scriptEngine.TransformAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_create_events_and_unset_schedule_if_failed()
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

            A.CallTo(() => scriptEngine.ExecuteAsync(A<ScriptVars>._, "<change-script>", ScriptOptions(), default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangeStatus_should_throw_exception_if_referenced_by_other_item()
        {
            var command = new ChangeContentStatus { Status = Status.Draft, CheckReferrers = true };

            await ExecuteCreateAsync();
            await ExecuteChangeStatusAsync(Status.Published);

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId, SearchScope.All, A<CancellationToken>._))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task ChangeStatus_should_not_throw_exception_if_referenced_by_other_item_but_forced()
        {
            var command = new ChangeContentStatus { Status = Status.Draft, CheckReferrers = false };

            await ExecuteCreateAsync();
            await ExecuteChangeStatusAsync(Status.Published);

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId, SearchScope.Published, A<CancellationToken>._))
                .Returns(true);

            await PublishAsync(command);
        }

        [Fact]
        public async Task Validate_should_not_update_state()
        {
            await ExecuteCreateAsync();

            var command = new ValidateContent();

            await PublishAsync(command);

            Assert.Equal(0, sut.Version);
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_deleted_flag()
        {
            await ExecuteCreateAsync();

            var command = new DeleteContent();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );

            A.CallTo(() => scriptEngine.ExecuteAsync(ScriptVars(data, null, Status.Draft), "<delete-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_not_create_events_if_permanent()
        {
            await ExecuteCreateAsync();

            var command = new DeleteContent { Permanent = true };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.Equal(EtagVersion.Empty, sut.Snapshot.Version);
            Assert.Empty(LastEvents);

            A.CallTo(() => scriptEngine.ExecuteAsync(ScriptVars(data, null, Status.Draft), "<delete-script>", ScriptOptions(), default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Delete_should_throw_exception_if_referenced_by_other_item()
        {
            await ExecuteCreateAsync();

            var command = new DeleteContent { CheckReferrers = true };

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId, SearchScope.All, A<CancellationToken>._))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));
        }

        [Fact]
        public async Task Delete_should_not_throw_exception_if_referenced_by_other_item_but_forced()
        {
            var command = new DeleteContent();

            await ExecuteCreateAsync();

            A.CallTo(() => contentRepository.HasReferrersAsync(AppId, contentId, SearchScope.All, A<CancellationToken>._))
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

            result.ShouldBeEquivalent(sut.Snapshot);

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

        private Task ExecuteDeleteAsync(bool permanent = false)
        {
            return PublishAsync(CreateContentCommand(new DeleteContent { Permanent = permanent }));
        }

        private Task ExecutePublishAsync()
        {
            return PublishAsync(CreateContentCommand(new ChangeContentStatus { Status = Status.Published }));
        }

        private static ScriptOptions ScriptOptions()
        {
            return A<ScriptOptions>.That.Matches(x => x.CanDisallow && x.CanReject && x.AsContext);
        }

        private ScriptVars ScriptVars(ContentData? newData, ContentData? oldData, Status newStatus)
        {
            return A<ScriptVars>.That.Matches(x => Matches(x, newData, oldData, newStatus, default));
        }

        private ScriptVars ScriptVars(ContentData? newData, ContentData? oldData, Status newStatus, Status oldStatus)
        {
            return A<ScriptVars>.That.Matches(x => Matches(x, newData, oldData, newStatus, oldStatus));
        }

        private bool Matches(ScriptVars x, ContentData? newData, ContentData? oldData, Status newStatus, Status oldStatus)
        {
            return
                Equals(x.Data, newData) &&
                Equals(x.DataOld, oldData) &&
                Equals(x.Status, newStatus) &&
                Equals(x.StatusOld, oldStatus) &&
                x.ContentId == contentId && x.User == User;
        }

        private T CreateContentEvent<T>(T @event) where T : ContentEvent
        {
            @event.ContentId = contentId;

            return CreateEvent(@event);
        }

        private T CreateContentCommand<T>(T command) where T : ContentCommand
        {
            command.ContentId = contentId;

            return CreateCommand(command);
        }

        private async Task<object> PublishAsync(ContentCommand command)
        {
            var result = await sut.ExecuteAsync(CreateContentCommand(command));

            return result.Payload;
        }
    }
}
