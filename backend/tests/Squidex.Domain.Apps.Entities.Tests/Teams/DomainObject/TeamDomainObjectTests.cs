// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
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
using Xunit;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject
{
    public class TeamDomainObjectTests : HandlerTestBase<TeamDomainObject.State>
    {
        private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
        private readonly IBillingManager billingManager = A.Fake<IBillingManager>();
        private readonly IUser user;
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly string contributorId = DomainId.NewGuid().ToString();
        private readonly string planIdPaid = "premium";
        private readonly string planIdFree = "free";
        private readonly string teamName = "My Team";
        private readonly DomainId teamId = DomainId.NewGuid();
        private readonly TeamDomainObject sut;

        protected override DomainId Id
        {
            get => teamId;
        }

        public TeamDomainObjectTests()
        {
            user = UserMocks.User(contributorId);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId, default))
                .Returns(user);

            A.CallTo(() => billingPlans.GetFreePlan())
                .Returns(new Plan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => billingPlans.GetPlan(planIdFree))
                .Returns(new Plan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => billingPlans.GetPlan(planIdPaid))
                .Returns(new Plan { Id = planIdPaid, MaxContributors = 30 });

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, A<string>._, default))
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
            var command = new CreateTeam { Name = teamName, TeamId = teamId };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(teamName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamCreated { Name = teamName }),
                    CreateTeamEvent(new TeamContributorAssigned { ContributorId = Actor.Identifier, Role = Role.Owner })
                );
        }

        [Fact]
        public async Task Create_should_not_assign_client_as_contributor()
        {
            var command = new CreateTeam { Name = teamName, Actor = ActorClient, TeamId = teamId };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(teamName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamCreated { Name = teamName }, true) // Must be with client actor.
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_label_and_description()
        {
            var command = new UpdateTeam { Name = "Changed Name" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(command.Name, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamUpdated { Name = command.Name })
                );
        }

        [Fact]
        public async Task ChangePlan_should_create_events_and_update_plan()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, planIdPaid, default))
                .Returns(Task.FromResult<Uri?>(null));

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdPaid));

            Assert.Equal(planIdPaid, sut.Snapshot.Plan!.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamPlanChanged { PlanId = planIdPaid })
                );

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, planIdPaid, default))
                .MustHaveHappened();

            A.CallTo(() => billingManager.SubscribeAsync(Actor.Identifier, teamId, planIdPaid, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_from_callback_should_create_events_and_update_plan()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdPaid));

            Assert.Equal(planIdPaid, sut.Snapshot.Plan!.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamPlanChanged { PlanId = planIdPaid })
                );

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<DomainId>._, A<string?>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => billingManager.SubscribeAsync(A<string>._, A<NamedId<DomainId>>._, A<string?>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree, FromCallback = true };

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdFree, true));

            Assert.Null(sut.Snapshot.Plan);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamPlanReset())
                );

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(A<string>._, A<NamedId<DomainId>>._, A<string?>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<NamedId<DomainId>>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_reset_plan_for_free_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree };

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdFree, true));

            Assert.Null(sut.Snapshot.Plan);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamPlanReset())
                );

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, planIdPaid, default))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => billingManager.UnsubscribeAsync(A<string>._, A<NamedId<DomainId>>._, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_not_make_update_for_redirect_result()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, planIdPaid, default))
                .Returns(new Uri("http://squidex.io"));

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdPaid, false, new Uri("http://squidex.io")));

            Assert.Null(sut.Snapshot.Plan);
        }

        [Fact]
        public async Task ChangePlan_should_not_call_billing_manager_for_callback()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new PlanChangedResult(planIdPaid));

            Assert.Equal(planIdPaid, sut.Snapshot.Plan?.PlanId);

            A.CallTo(() => billingManager.MustRedirectToPortalAsync(Actor.Identifier, teamId, planIdPaid, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => billingManager.SubscribeAsync(Actor.Identifier, teamId, planIdPaid, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task AssignContributor_should_create_events_and_add_contributor()
        {
            var command = new AssignContributor { ContributorId = contributorId, Role = Role.Editor };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Role.Editor, sut.Snapshot.Contributors[contributorId]);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamContributorAssigned { ContributorId = contributorId, Role = Role.Editor, IsAdded = true })
                );
        }

        [Fact]
        public async Task AssignContributor_should_create_update_events_and_update_contributor()
        {
            var command = new AssignContributor { ContributorId = contributorId, Role = Role.Owner };

            await ExecuteCreateAsync();
            await ExecuteAssignContributorAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Role.Owner, sut.Snapshot.Contributors[contributorId]);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamContributorAssigned { ContributorId = contributorId, Role = Role.Owner })
                );
        }

        [Fact]
        public async Task RemoveContributor_should_create_events_and_remove_contributor()
        {
            var command = new RemoveContributor { ContributorId = contributorId };

            await ExecuteCreateAsync();
            await ExecuteAssignContributorAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.Contributors.ContainsKey(contributorId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateTeamEvent(new TeamContributorRemoved { ContributorId = contributorId })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateTeam { Name = teamName, TeamId = teamId });
        }

        private Task ExecuteAssignContributorAsync()
        {
            return PublishAsync(new AssignContributor { ContributorId = contributorId, Role = Role.Editor });
        }

        private Task ExecuteChangePlanAsync()
        {
            return PublishAsync(new ChangePlan { PlanId = planIdPaid });
        }

        private T CreateTeamEvent<T>(T @event) where T : TeamEvent
        {
            @event.TeamId = teamId;

            return CreateEvent(@event);
        }

        private T CreateTeamCommand<T>(T command) where T : TeamCommand
        {
            command.TeamId = teamId;

            return CreateCommand(command);
        }

        private Task<object> PublishIdempotentAsync(TeamCommand command)
        {
            return PublishIdempotentAsync(sut, CreateTeamCommand(command));
        }

        private async Task<object> PublishAsync(TeamCommand command)
        {
            var result = await sut.ExecuteAsync(CreateTeamCommand(command), default);

            return result.Payload;
        }
    }
}
