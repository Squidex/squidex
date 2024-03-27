// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public class ContentDomainObjectTests : HandlerTestBase<WriteContent>
{
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();

    private readonly ContentData invalidData =
        new ContentData()
            .AddField("my-field1",
            new ContentFieldData()
                    .AddInvariant(JsonValue.Null))
            .AddField("my-field2",
                new ContentFieldData()
                    .AddInvariant(1));
    private readonly ContentData data =
        new ContentData()
            .AddField("my-field1",
                new ContentFieldData()
                    .AddInvariant(1));
    private readonly ContentData update =
        new ContentData()
            .AddField("my-field1",
                new ContentFieldData()
                    .AddInvariant(
                        JsonValue.Object()
                            .Add("$update", "$data['my-field1'].iv + 42")));
    private readonly ContentData updated =
        new ContentData()
            .AddField("my-field1",
                new ContentFieldData()
                    .AddInvariant(43));
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
        Schema = Schema
            .AddNumber(1, "my-field1", Partitioning.Invariant,
                    new NumberFieldProperties { IsRequired = true })
                .AddNumber(2, "my-field2", Partitioning.Invariant,
                    new NumberFieldProperties { IsRequired = false })
            .SetScripts(new SchemaScripts
            {
                Change = "<change-script>",
                Create = "<create-script>",
                Delete = "<delete-script>",
                Update = "<update-script>"
            })
            .Publish();

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, A<string>._, ScriptOptions(), CancellationToken))
            .ReturnsLazily(x => Task.FromResult(x.GetArgument<DataScriptVars>(0)!.Data!));

        A.CallTo(() => scriptEngine.Execute(A<ScriptVars>._, A<string>._, A<ScriptOptions>._))
            .Returns(JsonValue.Create(43));

        A.CallTo(() => contentWorkflow.GetInitialStatusAsync(Schema))
            .Returns(Status.Draft);

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Draft, Status.Published, A<ClaimsPrincipal?>._))
            .Returns(true);

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Draft, Status.Archived, A<ClaimsPrincipal?>._))
            .Returns(true);

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Published, Status.Draft, A<ClaimsPrincipal?>._))
            .Returns(true);

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Published, Status.Archived, A<ClaimsPrincipal?>._))
            .Returns(true);

        A.CallTo(() => contentWorkflow.CanUpdateAsync(A<Content>._, A<Status>._, A<ClaimsPrincipal?>._))
            .Returns(true);

        patched = patch.MergeInto(data);

        var log = A.Fake<ILogger<ContentDomainObject>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(A.Fake<ILogger<ContentValidator>>())
                .AddSingleton(log)
                .AddSingleton(contentWorkflow)
                .AddSingleton(contentRepository)
                .AddSingleton(scriptEngine)
                .AddSingleton(TestUtils.DefaultSerializer)
                .AddSingleton<IValidatorsFactory>(new DefaultValidatorsFactory())
                .BuildServiceProvider();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new ContentDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
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

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_should_not_change_status_if_set_to_initial()
    {
        var command = new CreateContent { Data = data, Status = Status.Draft };

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_should_change_status_if_set()
    {
        var command = new CreateContent { Data = data, Status = Status.Archived };

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
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

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_not_change_status_on_create_if_status_set_to_initial()
    {
        var command = new UpsertContent { Data = data };

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_change_status_on_create_if_status_set()
    {
        var command = new UpsertContent { Data = data, Status = Status.Archived };

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft), "<create-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_update_content_if_found()
    {
        var command = new UpsertContent { Data = otherData };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_patch_content_if_found()
    {
        var command = new UpsertContent { Data = patch, Patch = true };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(patched, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_not_change_status_on_update_if_status_set_to_initial()
    {
        var command = new UpsertContent { Data = otherData, Status = Status.Draft };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_change_status_on_update_if_status_set()
    {
        var command = new UpsertContent { Data = otherData, Status = Status.Archived };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(CreateContentCommand(command));

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Upsert_should_recreate_deleted_content()
    {
        var command = new UpsertContent { Data = data };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync();

        await PublishAsync(command);
    }

    [Fact]
    public async Task Upsert_should_recreate_permanently_deleted_content()
    {
        var command = new UpsertContent { Data = data };

        await ExecuteCreateAsync();
        await ExecuteDeleteAsync(true);

        await PublishAsync(command);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_data()
    {
        var command = new UpdateContent { Data = otherData };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_invoke_scripts()
    {
        var command = new UpdateContent { Data = update };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(updated, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_create_events_and_update_new_version_if_draft_available()
    {
        var command = new UpdateContent { Data = otherData };

        await ExecuteCreateAsync();
        await ExecutePublishAsync();
        await ExecuteCreateDraftAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(otherData, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_not_create_event_for_same_data()
    {
        var command = new UpdateContent { Data = data };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, "<update-script>", ScriptOptions(), CancellationToken))
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

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(patched, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Path_should_invoke_scripts()
    {
        var command = new PatchContent { Data = update };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(updated, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Patch_should_create_events_and_update_new_version_if_draft_available()
    {
        var command = new PatchContent { Data = patch };

        await ExecuteCreateAsync();
        await ExecutePublishAsync();
        await ExecuteCreateDraftAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(patched, data, Status.Draft), "<update-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Patch_should_not_create_event_for_same_data()
    {
        var command = new PatchContent { Data = data };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, "<update-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_update_status_if_published()
    {
        var command = new ChangeContentStatus { Status = Status.Archived };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_update_status_if_changed()
    {
        var command = new ChangeContentStatus { Status = Status.Archived };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_update_status_if_unpublished()
    {
        var command = new ChangeContentStatus { Status = Status.Draft };

        await ExecuteCreateAsync();
        await ExecutePublishAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_also_update_if_script_changes_data()
    {
        var command = new ChangeContentStatus { Status = Status.Draft };

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), CancellationToken))
            .Returns(otherData);

        await ExecuteCreateAsync();
        await ExecutePublishAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Draft, Status.Published), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_update_new_version_if_draft_available()
    {
        var command = new ChangeContentStatus { Status = Status.Archived };

        await ExecuteCreateAsync();
        await ExecutePublishAsync();
        await ExecuteCreateDraftAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Archived, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_delete_new_version_if_available()
    {
        var command = new ChangeContentStatus { Status = Status.Published };

        await ExecuteCreateAsync();
        await ExecutePublishAsync();
        await ExecuteCreateDraftAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(DataScriptVars(data, null, Status.Published, Status.Draft), "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_create_events_and_set_schedule_if_duetime_set()
    {
        var dueTime = Instant.MaxValue;

        var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTime };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_refresh_properties_and_unset_schedule_if_completed()
    {
        var dueTime = Instant.MaxValue;

        await ExecuteCreateAsync();
        await ExecuteChangeStatusAsync(Status.Archived, dueTime);

        var command = new ChangeContentStatus { Status = Status.Archived, StatusJobId = sut.Snapshot.ScheduleJob!.Id };

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Draft, Status.Archived, ApiContext.UserPrincipal))
            .Returns(true);

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.TransformAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_create_events_and_unset_schedule_if_failed()
    {
        var dueTime = Instant.MaxValue;

        await ExecuteCreateAsync();
        await ExecuteChangeStatusAsync(Status.Published, dueTime);

        var command = new ChangeContentStatus { Status = Status.Published, StatusJobId = sut.Snapshot.ScheduleJob!.Id };

        A.CallTo(() => contentWorkflow.CanMoveToAsync(A<Content>._, Status.Draft, Status.Published, ApiContext.UserPrincipal))
            .Returns(false);

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);

        A.CallTo(() => scriptEngine.ExecuteAsync(A<DataScriptVars>._, "<change-script>", ScriptOptions(), CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangeStatus_should_throw_exception_if_referenced_by_other_item()
    {
        var command = new ChangeContentStatus { Status = Status.Draft, CheckReferrers = true };

        await ExecuteCreateAsync();
        await ExecuteChangeStatusAsync(Status.Published);

        A.CallTo(() => contentRepository.HasReferrersAsync(App, contentId, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));
    }

    [Fact]
    public async Task ChangeStatus_should_not_throw_exception_if_referenced_by_other_item_but_forced()
    {
        var command = new ChangeContentStatus { Status = Status.Draft, CheckReferrers = false };

        await ExecuteCreateAsync();
        await ExecuteChangeStatusAsync(Status.Published);

        A.CallTo(() => contentRepository.HasReferrersAsync(App, contentId, SearchScope.Published, A<CancellationToken>._))
            .Returns(true);

        await PublishAsync(command);
    }

    [Fact]
    public async Task CancelContentSchedule_should_create_events_and_unset_schedule()
    {
        var command = new CancelContentSchedule();

        await ExecuteCreateAsync();
        await ExecuteChangeStatusAsync(Status.Published, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1)));

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Validate_should_not_update_state()
    {
        var command = new ValidateContent();

        await ExecuteCreateAsync();
        await PublishAsync(command);

        Assert.Equal(0, sut.Version);
    }

    [Fact]
    public async Task EnrichContentDefaults_should_update_content()
    {
        await ExecuteCreateAsync();

        Schema =
            Schema.AddString(3, "defaults", Partitioning.Invariant,
                new StringFieldProperties { DefaultValue = "Default Value" });

        var command = new EnrichContentDefaults();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task EnrichContentDefaults_should_not_update_content_if_all_fields_have_a_value()
    {
        await ExecuteCreateAsync();

        var command = new EnrichContentDefaults();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Delete_should_create_events_and_update_deleted_flag()
    {
        await ExecuteCreateAsync();

        var command = new DeleteContent();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual, None.Value);

        A.CallTo(() => scriptEngine.ExecuteAsync(DataScriptVars(data, null, Status.Draft), "<delete-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_not_create_events_if_permanent()
    {
        await ExecuteCreateAsync();

        var command = new DeleteContent { Permanent = true };

        var actual = await PublishAsync(command);

        Assert.Empty(LastEvents);

        A.CallTo(() => scriptEngine.ExecuteAsync(DataScriptVars(data, null, Status.Draft), "<delete-script>", ScriptOptions(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_throw_exception_if_referenced_by_other_item()
    {
        await ExecuteCreateAsync();

        var command = new DeleteContent { CheckReferrers = true };

        A.CallTo(() => contentRepository.HasReferrersAsync(App, contentId, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => PublishAsync(command));
    }

    [Fact]
    public async Task Delete_should_not_throw_exception_if_referenced_by_other_item_but_forced()
    {
        var command = new DeleteContent();

        await ExecuteCreateAsync();

        A.CallTo(() => contentRepository.HasReferrersAsync(App, contentId, SearchScope.All, A<CancellationToken>._))
            .Returns(true);

        await PublishAsync(command);
    }

    [Fact]
    public async Task CreateDraft_should_create_events_and_update_new_state()
    {
        var command = new CreateContentDraft();

        await ExecuteCreateAsync();
        await ExecutePublishAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteDraft_should_create_events_and_delete_new_version()
    {
        var command = new DeleteContentDraft();

        await ExecuteCreateAsync();
        await ExecutePublishAsync();
        await ExecuteCreateDraftAsync();

        var actual = await PublishAsync(command);

        await VerifySutAsync(actual);
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

    private DataScriptVars DataScriptVars(ContentData? newData, ContentData? oldData, Status newStatus)
    {
        return A<DataScriptVars>.That.Matches(x => Matches(x, newData, oldData, newStatus, default));
    }

    private DataScriptVars DataScriptVars(ContentData? newData, ContentData? oldData, Status newStatus, Status oldStatus)
    {
        return A<DataScriptVars>.That.Matches(x => Matches(x, newData, oldData, newStatus, oldStatus));
    }

    private bool Matches(DataScriptVars x, ContentData? newData, ContentData? oldData, Status newStatus, Status oldStatus)
    {
        return
            Equals(x["contentId"], contentId) &&
            Equals(x["data"], newData) &&
            Equals(x["dataOld"], oldData) &&
            Equals(x["status"], newStatus) &&
            Equals(x["statusOld"], oldStatus) &&
            Equals(x["user"], ApiContext.UserPrincipal);
    }

    private T CreateContentEvent<T>(T @event) where T : ContentEvent
    {
        @event.ContentId = contentId;

        return CreateEvent(@event);
    }

    private T CreateContentCommand<T>(T command) where T : ContentCommand
    {
        command.ContentId = contentId;

        return (T)CreateCommand(command);
    }

    private Task<object> PublishAsync(ContentCommand command)
    {
        return PublishAsync(sut, CreateContentCommand(command));
    }

    private async Task VerifySutAsync(object? actual, object? expected = null)
    {
        if (expected == null)
        {
            actual.Should().BeEquivalentTo(sut.Snapshot, o => o.IncludingProperties());
        }
        else
        {
            actual.Should().BeEquivalentTo(expected);
        }

        Assert.Equal(AppId, sut.Snapshot.AppId);

        await Verify(new { sut, events = LastEvents });
    }
}
