// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public class AppDomainObjectTests : HandlerTestBase<App>
{
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly IBillingManager billingManager = A.Fake<IBillingManager>();
    private readonly IUser user;
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly IUsageGate usageGate = A.Fake<IUsageGate>();
    private readonly string contributorId = DomainId.NewGuid().ToString();
    private readonly string clientId = "client";
    private readonly string clientNewName = "My Client";
    private readonly string roleName = "My Role";
    private readonly string planIdPaid = "premium";
    private readonly string planIdFree = "free";
    private readonly InitialSettings initialSettings;
    private readonly DomainId workflowId = DomainId.NewGuid();
    private readonly AppDomainObject sut;

    protected override DomainId Id
    {
        get => AppId.Id;
    }

    public AppDomainObjectTests()
    {
        user = UserMocks.User(contributorId);

        Team = Team with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Owner)
        };

        A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId, CancellationToken))
            .Returns(user);

        A.CallTo(() => usageGate.GetPlanForAppAsync(A<App>.That.Matches(x => x.Plan != null && x.Plan.PlanId == planIdFree), false, CancellationToken))
            .Returns((new Plan { Id = planIdFree, MaxContributors = 10 }, planIdFree, null));

        A.CallTo(() => usageGate.GetPlanForAppAsync(A<App>.That.Matches(x => x.Plan != null && x.Plan.PlanId == planIdPaid), false, CancellationToken))
            .Returns((new Plan { Id = planIdPaid, MaxContributors = 30 }, planIdPaid, null));

        A.CallTo(() => billingPlans.GetFreePlan())
            .Returns(new Plan { Id = planIdFree, MaxContributors = 10 });

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, A<string>._, CancellationToken))
            .Returns(Task.FromResult<Uri?>(null));

        // Create a non-empty setting, otherwise the event is not raised as it does not change the domain object.
        initialSettings = new InitialSettings
        {
            Settings = new AppSettings
            {
                HideScheduler = true
            }
        };

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(billingManager)
                .AddSingleton(billingPlans)
                .AddSingleton(initialSettings)
                .AddSingleton(usageGate)
                .AddSingleton(userResolver)
                .BuildServiceProvider();

        var log = A.Fake<ILogger<AppDomainObject>>();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new AppDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    [Fact]
    public async Task Command_should_throw_exception_if_app_is_deleted()
    {
        await ExecuteCreateAsync();
        await ExecuteArchiveAsync();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(ExecuteAttachClientAsync);
    }

    [Fact]
    public async Task Create_should_create_events_and_set_intitial_state()
    {
        var command = new CreateApp { Name = AppId.Name, AppId = AppId.Id };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Create_should_not_assign_client_as_contributor()
    {
        var command = new CreateApp { Name = AppId.Name, Actor = Client, AppId = AppId.Id };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_label_and_description()
    {
        var command = new UpdateApp { Label = "my-label", Description = "my-description" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateSettings_should_create_event_and_update_settings()
    {
        var command = new UpdateAppSettings
        {
            Settings = new AppSettings
            {
                HideDateTimeModeButton = true
            }
        };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UploadImage_should_create_events_and_update_image()
    {
        var command = new UploadAppImage { File = new NoopAssetFile() };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task RemoveImage_should_create_events_and_update_image()
    {
        var command = new RemoveAppImage();

        await ExecuteCreateAsync();
        await ExecuteUploadImage();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ChangePlan_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planIdPaid };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, planIdPaid, default))
            .Returns(Task.FromResult<Uri?>(null));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdPaid));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, planIdPaid, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<App>._, planIdPaid, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdPaid));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<App>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(A<string>._, A<App>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planIdFree, FromCallback = true };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdFree, true));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._,  A<App>._, A<string?>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._,  A<App>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planIdFree };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdFree, true));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, planIdPaid, CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._,  A<App>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_not_make_update_for_redirect()
    {
        var command = new ChangePlan { PlanId = planIdPaid };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, planIdPaid, CancellationToken))
            .Returns(new Uri("http://squidex.io"));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdPaid, false, new Uri("http://squidex.io")));
    }

    [Fact]
    public async Task ChangePlan_should_not_call_billing_manager_for_callback()
    {
        var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planIdPaid));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<App>._, planIdPaid, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<App>._, planIdPaid, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AssignContributor_should_create_events_and_add_contributor()
    {
        var command = new AssignContributor { ContributorId = contributorId, Role = Role.Editor };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task AssignContributor_should_create_update_events_and_update_contributor()
    {
        var command = new AssignContributor { ContributorId = contributorId, Role = Role.Owner };

        await ExecuteCreateAsync();
        await ExecuteAssignContributorAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task RemoveContributor_should_create_events_and_remove_contributor()
    {
        var command = new RemoveContributor { ContributorId = contributorId };

        await ExecuteCreateAsync();
        await ExecuteAssignContributorAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Transfer_should_create_events_and_set_team()
    {
        var command = new TransferToTeam { TeamId = TeamId };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Transfer_from_team_should_create_events_and_set_team()
    {
        var command = new TransferToTeam { TeamId = null };

        await ExecuteCreateAsync();
        await ExecuteTransferAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task AttachClient_should_create_events_and_add_client()
    {
        var command = new AttachClient { Id = clientId };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateClient_should_create_events_and_update_client()
    {
        var command = new UpdateClient { Id = clientId, Name = clientNewName, Role = Role.Developer };

        await ExecuteCreateAsync();
        await ExecuteAttachClientAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task RevokeClient_should_create_events_and_remove_client()
    {
        var command = new RevokeClient { Id = clientId };

        await ExecuteCreateAsync();
        await ExecuteAttachClientAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task AddWorkflow_should_create_events_and_add_workflow()
    {
        var command = new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateWorkflow_should_create_events_and_update_workflow()
    {
        var command = new UpdateWorkflow { WorkflowId = workflowId, Workflow = Workflow.Default };

        await ExecuteCreateAsync();
        await ExecuteAddWorkflowAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteWorkflow_should_create_events_and_remove_workflow()
    {
        var command = new DeleteWorkflow { WorkflowId = workflowId };

        await ExecuteCreateAsync();
        await ExecuteAddWorkflowAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task AddLanguage_should_create_events_and_add_language()
    {
        var command = new AddLanguage { Language = Language.DE };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task RemoveLanguage_should_create_events_and_remove_language()
    {
        var command = new RemoveLanguage { Language = Language.DE };

        await ExecuteCreateAsync();
        await ExecuteAddLanguageAsync(Language.DE);

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateLanguage_should_create_events_and_update_language()
    {
        var command = new UpdateLanguage { Language = Language.DE, Fallback = [Language.EN] };

        await ExecuteCreateAsync();
        await ExecuteAddLanguageAsync(Language.DE);

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task AddRole_should_create_events_and_add_role()
    {
        var command = new AddRole { Name = roleName };

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteRole_should_create_events_and_delete_role()
    {
        var command = new DeleteRole { Name = roleName };

        await ExecuteCreateAsync();
        await ExecuteAddRoleAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpdateRole_should_create_events_and_update_role()
    {
        var command = new UpdateRole { Name = roleName, Permissions = ["clients.read"], Properties = JsonValue.Object() };

        await ExecuteCreateAsync();
        await ExecuteAddRoleAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task DeleteApp_should_create_events_and_update_deleted_flag()
    {
        var command = new DeleteApp();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);

        A.CallTo(() => billingManager.UnsubscribeAsync(command.Actor.Identifier, A<App>._, default))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, new CreateApp { Name = AppId.Name, AppId = AppId.Id });
    }

    private Task ExecuteUploadImage()
    {
        return PublishAsync(sut, new UploadAppImage { File = new NoopAssetFile() });
    }

    private Task ExecuteAssignContributorAsync()
    {
        return PublishAsync(sut, new AssignContributor { ContributorId = contributorId, Role = Role.Editor });
    }

    private Task ExecuteAttachClientAsync()
    {
        return PublishAsync(sut, new AttachClient { Id = clientId });
    }

    private Task ExecuteAddRoleAsync()
    {
        return PublishAsync(sut, new AddRole { Name = roleName });
    }

    private Task ExecuteAddLanguageAsync(Language language)
    {
        return PublishAsync(sut, new AddLanguage { Language = language });
    }

    private Task ExecuteAddWorkflowAsync()
    {
        return PublishAsync(sut, new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" });
    }

    private Task ExecuteChangePlanAsync()
    {
        return PublishAsync(sut, new ChangePlan { PlanId = planIdPaid });
    }

    private Task ExecuteTransferAsync()
    {
        return PublishAsync(sut, new TransferToTeam { TeamId = TeamId });
    }

    private Task ExecuteArchiveAsync()
    {
        return PublishAsync(sut, new DeleteApp());
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

        await Verify(new { sut, events = LastEvents });
    }
}
