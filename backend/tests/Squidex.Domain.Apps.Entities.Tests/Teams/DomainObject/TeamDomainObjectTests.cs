// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject;

public class TeamDomainObjectTests : HandlerTestBase<TeamDomainObject.State>
{
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly IBillingManager billingManager = A.Fake<IBillingManager>();
    private readonly IUser user;
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly Plan planPaid = new Plan { Id = "premium" };
    private readonly Plan planFree = new Plan { Id = "free" };
    private readonly string contributorId = DomainId.NewGuid().ToString();
    private readonly string name = "My Team";
    private readonly TeamDomainObject sut;

    protected override DomainId Id
    {
        get => TeamId;
    }

    public TeamDomainObjectTests()
    {
        user = UserMocks.User(contributorId);

        A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId, default))
            .Returns(user);

        A.CallTo(() => billingPlans.GetFreePlan())
            .Returns(planFree);

        A.CallTo(() => billingPlans.GetPlan(planFree.Id))
            .Returns(planFree);

        A.CallTo(() => billingPlans.GetPlan(planPaid.Id))
            .Returns(planPaid);

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, A<string>._, CancellationToken))
            .Returns(Task.FromResult<Uri?>(null));

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(billingPlans)
                .AddSingleton(billingManager)
                .AddSingleton(userResolver)
                .BuildServiceProvider();

        var log = A.Fake<ILogger<TeamDomainObject>>();

#pragma warning disable MA0056 // Do not call overridable members in constructor
        sut = new TeamDomainObject(Id, PersistenceFactory, log, serviceProvider);
#pragma warning restore MA0056 // Do not call overridable members in constructor
    }

    [Fact]
    public async Task Create_should_create_events_and_set_intitial_state()
    {
        var command = new CreateTeam { Name = name, TeamId = TeamId };

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(name, sut.Snapshot.Name);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamCreated { Name = name }),
                CreateEvent(new TeamContributorAssigned { ContributorId = User.Identifier, Role = Role.Owner })
            );
    }

    [Fact]
    public async Task Create_should_not_assign_client_as_contributor()
    {
        var command = new CreateTeam { Name = name, Actor = Client, TeamId = TeamId };

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(name, sut.Snapshot.Name);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamCreated { Name = name }, true) // Must be with client User.
            );
    }

    [Fact]
    public async Task Update_should_create_events_and_update_label_and_description()
    {
        var command = new UpdateTeam { Name = "Changed Name" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.Name, sut.Snapshot.Name);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamUpdated { Name = command.Name })
            );
    }

    [Fact]
    public async Task ChangePlan_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planPaid.Id };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, CancellationToken))
            .Returns(Task.FromResult<Uri?>(null));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planPaid.Id));

        Assert.Equal(planPaid.Id, sut.Snapshot.Plan!.PlanId);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamPlanChanged { PlanId = planPaid.Id })
            );

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planPaid.Id, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planPaid.Id));

        Assert.Equal(planPaid.Id, sut.Snapshot.Plan!.PlanId);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamPlanChanged { PlanId = planPaid.Id })
            );

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<ITeamEntity>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(A<string>._, A<ITeamEntity>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planFree.Id, FromCallback = true };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planFree.Id, true));

        Assert.Null(sut.Snapshot.Plan);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamPlanReset())
            );

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<ITeamEntity>._, A<string?>._, CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<ITeamEntity>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planFree.Id };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planFree.Id, true));

        Assert.Null(sut.Snapshot.Plan);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamPlanReset())
            );

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<ITeamEntity>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_not_make_update_for_redirect_actual()
    {
        var command = new ChangePlan { PlanId = planPaid.Id };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, CancellationToken))
            .Returns(new Uri("http://squidex.io"));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planPaid.Id, false, new Uri("http://squidex.io")));

        Assert.Null(sut.Snapshot.Plan);
    }

    [Fact]
    public async Task ChangePlan_should_not_call_billing_manager_for_callback()
    {
        var command = new ChangePlan { PlanId = planPaid.Id, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(new PlanChangedResult(planPaid.Id));

        Assert.Equal(planPaid.Id, sut.Snapshot.Plan?.PlanId);

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<ITeamEntity>._, planPaid.Id, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AssignContributor_should_create_events_and_add_contributor()
    {
        var command = new AssignContributor { ContributorId = contributorId };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.Equal(command.Role, sut.Snapshot.Contributors[contributorId]);

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamContributorAssigned { ContributorId = contributorId, Role = command.Role, IsAdded = true })
            );
    }

    [Fact]
    public async Task RemoveContributor_should_create_events_and_remove_contributor()
    {
        var command = new RemoveContributor { ContributorId = contributorId };

        await ExecuteCreateAsync();
        await ExecuteAssignContributorAsync();

        var actual = await PublishAsync(command);

        actual.ShouldBeEquivalent(sut.Snapshot);

        Assert.False(sut.Snapshot.Contributors.ContainsKey(contributorId));

        LastEvents
            .ShouldHaveSameEvents(
                CreateEvent(new TeamContributorRemoved { ContributorId = contributorId })
            );
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(new CreateTeam { Name = name, TeamId = TeamId });
    }

    private Task ExecuteAssignContributorAsync()
    {
        return PublishAsync(new AssignContributor { ContributorId = contributorId });
    }

    private Task ExecuteChangePlanAsync()
    {
        return PublishAsync(new ChangePlan { PlanId = planPaid.Id });
    }

    private Task<object> PublishIdempotentAsync(TeamCommand command)
    {
        return PublishIdempotentAsync(sut, CreateCommand(command));
    }

    private async Task<object> PublishAsync(TeamCommand command)
    {
        var actual = await sut.ExecuteAsync(CreateCommand(command), CancellationToken);

        return actual.Payload;
    }
}
