// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject;

public class TeamDomainObjectTests : HandlerTestBase<Team>
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly IBillingManager billingManager = A.Fake<IBillingManager>();
    private readonly IUser user;
    private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    private readonly Plan planPaid = new Plan { Id = "premium" };
    private readonly Plan planFree = new Plan { Id = "free" };
    private readonly string contributorId = DomainId.NewGuid().ToString();
    private readonly string name = "My Team";
    private readonly AuthScheme scheme;
    private readonly TeamDomainObject sut;

    protected override DomainId Id
    {
        get => TeamId;
    }

    public TeamDomainObjectTests()
    {
        user = UserMocks.User(contributorId);

        A.CallTo(() => appProvider.GetTeamByAuthDomainAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Team?>(null));

        A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId, default))
            .Returns(user);

        A.CallTo(() => billingPlans.GetFreePlan())
            .Returns(planFree);

        A.CallTo(() => billingPlans.GetPlan(planFree.Id))
            .Returns(planFree);

        A.CallTo(() => billingPlans.GetPlan(planPaid.Id))
            .Returns(planPaid);

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, A<string>._, CancellationToken))
            .Returns(Task.FromResult<Uri?>(null));

        scheme = new AuthScheme
        {
            Domain = "squidex.io",
            DisplayName = "Squidex",
            Authority = "https://identity.squidex.io",
            ClientId = "clientId",
            ClientSecret = "clientSecret"
        };

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
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

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Create_should_not_assign_client_as_contributor()
    {
        var command = new CreateTeam { Name = name, Actor = Client, TeamId = TeamId };

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task Update_should_create_events_and_update_label_and_description()
    {
        var command = new UpdateTeam { Name = "Changed Name" };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpsertAuth_should_create_events_and_update_scheme()
    {
        var command = new UpsertAuth { Scheme = scheme };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task UpsertAuth_should_create_events_and_remove_scheme()
    {
        var command = new UpsertAuth { Scheme = null };

        await ExecuteCreateAsync();
        await ExecuteUpsertAuthAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual);
    }

    [Fact]
    public async Task ChangePlan_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planPaid.Id };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, planPaid.Id, CancellationToken))
            .Returns(Task.FromResult<Uri?>(null));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planPaid.Id));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, planPaid.Id, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<Team>._, planPaid.Id, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_create_events_and_update_plan()
    {
        var command = new ChangePlan { PlanId = planPaid.Id, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planPaid.Id));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<Team>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(A<string>._, A<Team>._, A<string?>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planFree.Id, FromCallback = true };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planFree.Id, true));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<Team>._, A<string?>._, CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<Team>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_reset_plan_for_free_plan()
    {
        var command = new ChangePlan { PlanId = planFree.Id };

        await ExecuteCreateAsync();
        await ExecuteChangePlanAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planFree.Id, true));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, planPaid.Id, CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<Team>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ChangePlan_should_not_make_update_for_redirect_result()
    {
        var command = new ChangePlan { PlanId = planPaid.Id };

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, planPaid.Id, CancellationToken))
            .Returns(new Uri("http://squidex.io"));

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planPaid.Id, false, new Uri("http://squidex.io")));
    }

    [Fact]
    public async Task ChangePlan_should_not_call_billing_manager_for_callback()
    {
        var command = new ChangePlan { PlanId = planPaid.Id, FromCallback = true };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await VerifySutAsync(actual, new PlanChangedResult(planPaid.Id));

        A.CallTo(() => billingManager.MustRedirectToPortalAsync(User.Identifier, A<Team>._, planPaid.Id, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => billingManager.SubscribeAsync(User.Identifier, A<Team>._, planPaid.Id, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task AssignContributor_should_create_events_and_add_contributor()
    {
        var command = new AssignContributor { ContributorId = contributorId };

        await ExecuteCreateAsync();

        var actual = await PublishIdempotentAsync(sut, command);

        await Verify(actual);
    }

    [Fact]
    public async Task RemoveContributor_should_create_events_and_remove_contributor()
    {
        var command = new RemoveContributor { ContributorId = contributorId };

        await ExecuteCreateAsync();
        await ExecuteAssignContributorAsync();

        var actual = await PublishAsync(sut, command);

        await Verify(actual);
    }

    [Fact]
    public async Task DeleteTeam_should_create_events_and_update_deleted_flag()
    {
        var command = new DeleteTeam();

        await ExecuteCreateAsync();

        var actual = await PublishAsync(sut, command);

        await VerifySutAsync(actual, None.Value);

        A.CallTo(() => billingManager.UnsubscribeAsync(command.Actor.Identifier, A<Team>._, default))
            .MustHaveHappened();
    }

    private Task ExecuteCreateAsync()
    {
        return PublishAsync(sut, new CreateTeam { Name = name, TeamId = TeamId });
    }

    private Task ExecuteAssignContributorAsync()
    {
        return PublishAsync(sut, new AssignContributor { ContributorId = contributorId });
    }

    private Task ExecuteChangePlanAsync()
    {
        return PublishAsync(sut, new ChangePlan { PlanId = planPaid.Id });
    }

    private Task ExecuteUpsertAuthAsync()
    {
        return PublishAsync(sut, new UpsertAuth { Scheme = scheme });
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
