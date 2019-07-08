// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppGrainTests : HandlerTestBase<AppState>
    {
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IAppPlanBillingManager appPlansBillingManager = A.Fake<IAppPlanBillingManager>();
        private readonly IUser user = A.Fake<IUser>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly string contributorId = Guid.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";
        private readonly string roleName = "My Role";
        private readonly string planIdPaid = "premium";
        private readonly string planIdFree = "free";
        private readonly AppGrain sut;
        private readonly Guid workflowId = Guid.NewGuid();
        private readonly Guid patternId1 = Guid.NewGuid();
        private readonly Guid patternId2 = Guid.NewGuid();
        private readonly Guid patternId3 = Guid.NewGuid();
        private readonly InitialPatterns initialPatterns;

        protected override Guid Id
        {
            get { return AppId; }
        }

        public AppGrainTests()
        {
            A.CallTo(() => user.Id)
                .Returns(contributorId);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId))
                .Returns(user);

            A.CallTo(() => appPlansProvider.GetPlan(A<string>.Ignored))
                .Returns(new ConfigAppLimitsPlan { MaxContributors = 10 });

            initialPatterns = new InitialPatterns
            {
                { patternId1, new AppPattern("Number", "[0-9]") },
                { patternId2, new AppPattern("Numbers", "[0-9]*") }
            };

            sut = new AppGrain(initialPatterns, Store, A.Dummy<ISemanticLog>(), appPlansProvider, appPlansBillingManager, userResolver);
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_app_is_archived()
        {
            await ExecuteCreateAsync();
            await ExecuteArchiveAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteAttachClientAsync);
        }

        [Fact]
        public async Task Create_should_create_events_and_update_state()
        {
            var command = new CreateApp { Name = AppName, Actor = Actor, AppId = AppId };

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(AppName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }),
                    CreateEvent(new AppContributorAssigned { ContributorId = Actor.Identifier, Role = Role.Owner }),
                    CreateEvent(new AppLanguageAdded { Language = Language.EN }),
                    CreateEvent(new AppPatternAdded { PatternId = patternId1, Name = "Number", Pattern = "[0-9]" }),
                    CreateEvent(new AppPatternAdded { PatternId = patternId2, Name = "Numbers", Pattern = "[0-9]*" })
                );
        }

        [Fact]
        public async Task ChangePlan_should_create_events_and_update_state()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new PlanChangedResult());

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            Assert.True(result.Value is PlanChangedResult);

            Assert.Equal(planIdPaid, sut.Snapshot.Plan.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanChanged { PlanId = planIdPaid })
                );
        }

        [Fact]
        public async Task ChangePlan_should_reset_plan_for_reset_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new PlanChangedResult());

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdFree))
                .Returns(new PlanResetResult());

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            Assert.True(result.Value is PlanResetResult);

            Assert.Null(sut.Snapshot.Plan);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanReset())
                );
        }

        [Fact]
        public async Task ChangePlan_should_not_make_update_for_redirect_result()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new RedirectToCheckoutResult(new Uri("http://squidex.io")));

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new RedirectToCheckoutResult(new Uri("http://squidex.io")));

            Assert.Null(sut.Snapshot.Plan);
        }

        [Fact]
        public async Task ChangePlan_should_not_call_billing_manager_for_callback()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(5));

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task AssignContributor_should_create_events_and_update_state()
        {
            var command = new AssignContributor { ContributorId = contributorId, Role = Role.Editor };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Role.Editor, sut.Snapshot.Contributors[contributorId]);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Role = Role.Editor, IsAdded = true })
                );
        }

        [Fact]
        public async Task AssignContributor_should_create_update_events_and_update_state()
        {
            var command = new AssignContributor { ContributorId = contributorId, Role = Role.Owner };

            await ExecuteCreateAsync();
            await ExecuteAssignContributorAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(Role.Owner, sut.Snapshot.Contributors[contributorId]);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Role = Role.Owner })
                );
        }

        [Fact]
        public async Task RemoveContributor_should_create_events_and_update_state()
        {
            var command = new RemoveContributor { ContributorId = contributorId };

            await ExecuteCreateAsync();
            await ExecuteAssignContributorAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.Contributors.ContainsKey(contributorId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppContributorRemoved { ContributorId = contributorId })
                );
        }

        [Fact]
        public async Task AttachClient_should_create_events_and_update_state()
        {
            var command = new AttachClient { Id = clientId };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.Clients.ContainsKey(clientId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientAttached { Id = clientId, Secret = command.Secret })
                );
        }

        [Fact]
        public async Task UpdateClient_should_create_events_and_update_state()
        {
            var command = new UpdateClient { Id = clientId, Name = clientNewName, Role = Role.Developer };

            await ExecuteCreateAsync();
            await ExecuteAttachClientAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(clientNewName, sut.Snapshot.Clients[clientId].Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRenamed { Id = clientId, Name = clientNewName }),
                    CreateEvent(new AppClientUpdated { Id = clientId, Role = Role.Developer })
                );
        }

        [Fact]
        public async Task RevokeClient_should_create_events_and_update_state()
        {
            var command = new RevokeClient { Id = clientId };

            await ExecuteCreateAsync();
            await ExecuteAttachClientAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.Clients.ContainsKey(clientId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRevoked { Id = clientId })
                );
        }

        [Fact]
        public async Task AddWorkflow_should_create_events_and_update_state()
        {
            var command = new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEmpty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowAdded { WorkflowId = workflowId, Name = "my-workflow" })
                );
        }

        [Fact]
        public async Task UpdateWorkflow_should_create_events_and_update_state()
        {
            var command = new UpdateWorkflow { WorkflowId = workflowId, Workflow = Workflow.Default };

            await ExecuteCreateAsync();
            await ExecuteAddWorkflowAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEmpty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowUpdated { WorkflowId = workflowId, Workflow = Workflow.Default })
                );
        }

        [Fact]
        public async Task DeleteWorkflow_should_create_events_and_update_state()
        {
            var command = new DeleteWorkflow { WorkflowId = workflowId };

            await ExecuteCreateAsync();
            await ExecuteAddWorkflowAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Empty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowDeleted { WorkflowId = workflowId })
                );
        }

        [Fact]
        public async Task AddLanguage_should_create_events_and_update_state()
        {
            var command = new AddLanguage { Language = Language.DE };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageAdded { Language = Language.DE })
                );
        }

        [Fact]
        public async Task RemoveLanguage_should_create_events_and_update_state()
        {
            var command = new RemoveLanguage { Language = Language.DE };

            await ExecuteCreateAsync();
            await ExecuteAddLanguageAsync(Language.DE);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageRemoved { Language = Language.DE })
                );
        }

        [Fact]
        public async Task UpdateLanguage_should_create_events_and_update_state()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new List<Language> { Language.EN } };

            await ExecuteCreateAsync();
            await ExecuteAddLanguageAsync(Language.DE);

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.LanguagesConfig.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageUpdated { Language = Language.DE, Fallback = new List<Language> { Language.EN } })
                );
        }

        [Fact]
        public async Task AddRole_should_create_events_and_update_state()
        {
            var command = new AddRole { Name = roleName };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(5, sut.Snapshot.Roles.Count);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleAdded { Name = roleName })
                );
        }

        [Fact]
        public async Task DeleteRole_should_create_events_and_update_state()
        {
            var command = new DeleteRole { Name = roleName };

            await ExecuteCreateAsync();
            await ExecuteAddRoleAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(4, sut.Snapshot.Roles.Count);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleDeleted { Name = roleName })
                );
        }

        [Fact]
        public async Task UpdateRole_should_create_events_and_update_state()
        {
            var command = new UpdateRole { Name = roleName, Permissions = new[] { "clients.read" } };

            await ExecuteCreateAsync();
            await ExecuteAddRoleAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleUpdated { Name = roleName, Permissions = new[] { "clients.read" } })
                );
        }

        [Fact]
        public async Task AddPattern_should_create_events_and_update_state()
        {
            var command = new AddPattern { PatternId = patternId3, Name = "Any", Pattern = ".*", Message = "Msg" };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(initialPatterns.Count + 1, sut.Snapshot.Patterns.Count);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternAdded { PatternId = patternId3, Name = "Any", Pattern = ".*", Message = "Msg" })
                );
        }

        [Fact]
        public async Task DeletePattern_should_create_events_and_update_state()
        {
            var command = new DeletePattern { PatternId = patternId3 };

            await ExecuteCreateAsync();
            await ExecuteAddPatternAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(initialPatterns.Count, sut.Snapshot.Patterns.Count);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternDeleted { PatternId = patternId3 })
                );
        }

        [Fact]
        public async Task UpdatePattern_should_create_events_and_update_state()
        {
            var command = new UpdatePattern { PatternId = patternId3, Name = "Any", Pattern = ".*", Message = "Msg" };

            await ExecuteCreateAsync();
            await ExecuteAddPatternAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPatternUpdated { PatternId = patternId3, Name = "Any", Pattern = ".*", Message = "Msg" })
                );
        }

        [Fact]
        public async Task ArchiveApp_should_create_events_and_update_state()
        {
            var command = new ArchiveApp();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(5));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppArchived())
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, AppNamedId, null))
                .MustHaveHappened();
        }

        private Task ExecuteAddPatternAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AddPattern { PatternId = patternId3, Name = "Name", Pattern = ".*" }));
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new CreateApp { Name = AppName }));
        }

        private Task ExecuteAssignContributorAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AssignContributor { ContributorId = contributorId, Role = Role.Editor }));
        }

        private Task ExecuteAttachClientAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AttachClient { Id = clientId }));
        }

        private Task ExecuteAddRoleAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AddRole { Name = roleName }));
        }

        private Task ExecuteAddLanguageAsync(Language language)
        {
            return sut.ExecuteAsync(CreateCommand(new AddLanguage { Language = language }));
        }

        private Task ExecuteAddWorkflowAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" }));
        }

        private Task ExecuteChangePlanAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new ChangePlan { PlanId = planIdPaid }));
        }

        private Task ExecuteArchiveAsync()
        {
            return sut.ExecuteAsync(CreateCommand(new ArchiveApp()));
        }
    }
}
