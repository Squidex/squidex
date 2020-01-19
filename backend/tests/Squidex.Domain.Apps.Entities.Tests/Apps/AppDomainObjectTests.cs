﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
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
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppDomainObjectTests : HandlerTestBase<AppState>
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
        private readonly AppDomainObject sut;
        private readonly Guid workflowId = Guid.NewGuid();
        private readonly Guid patternId1 = Guid.NewGuid();
        private readonly Guid patternId2 = Guid.NewGuid();
        private readonly Guid patternId3 = Guid.NewGuid();
        private readonly InitialPatterns initialPatterns;

        protected override Guid Id
        {
            get { return AppId; }
        }

        public AppDomainObjectTests()
        {
            A.CallTo(() => user.Id)
                .Returns(contributorId);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId))
                .Returns(user);

            A.CallTo(() => appPlansProvider.GetFreePlan())
                .Returns(new ConfigAppLimitsPlan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => appPlansProvider.GetPlan(planIdFree))
                .Returns(new ConfigAppLimitsPlan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => appPlansProvider.GetPlan(planIdPaid))
                .Returns(new ConfigAppLimitsPlan { Id = planIdPaid, MaxContributors = 30 });

            initialPatterns = new InitialPatterns
            {
                { patternId1, new AppPattern("Number", "[0-9]") },
                { patternId2, new AppPattern("Numbers", "[0-9]*") }
            };

            sut = new AppDomainObject(initialPatterns, Store, A.Dummy<ISemanticLog>(), appPlansProvider, appPlansBillingManager, userResolver);
            sut.Setup(Id);
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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(AppName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }),
                    CreateEvent(new AppContributorAssigned { ContributorId = Actor.Identifier, Role = Role.Owner }),
                    CreateEvent(new AppPatternAdded { PatternId = patternId1, Name = "Number", Pattern = "[0-9]" }),
                    CreateEvent(new AppPatternAdded { PatternId = patternId2, Name = "Numbers", Pattern = "[0-9]*" })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var command = new UpdateApp { Label = "my-label", Description = "my-description" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal("my-label", sut.Snapshot.Label);
            Assert.Equal("my-description", sut.Snapshot.Description);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppUpdated { Label = "my-label", Description = "my-description" })
                );
        }

        [Fact]
        public async Task UploadImage_should_create_events_and_update_state()
        {
            var command = new UploadAppImage { File = new AssetFile("image.png", "image/png", 100, () => new MemoryStream()) };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal("image/png", sut.Snapshot.Image!.MimeType);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppImageUploaded { Image = sut.Snapshot.Image })
                );
        }

        [Fact]
        public async Task RemoveImage_should_create_events_and_update_state()
        {
            var command = new RemoveAppImage();

            await ExecuteCreateAsync();
            await ExecuteUploadImage();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Null(sut.Snapshot.Image);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppImageRemoved())
                );
        }

        [Fact]
        public async Task ChangePlan_should_create_events_and_update_state()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new PlanChangedResult());

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            Assert.True(result is PlanChangedResult);

            Assert.Equal(planIdPaid, sut.Snapshot.Plan!.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanChanged { PlanId = planIdPaid })
                );
        }

        [Fact]
        public async Task ChangePlan_from_callback_should_create_events_and_update_state()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(new EntitySavedResult(4));

            Assert.Equal(planIdPaid, sut.Snapshot.Plan!.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanChanged { PlanId = planIdPaid })
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(A<string>.Ignored, A<NamedId<Guid>>.Ignored, A<string?>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree, FromCallback = true };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new PlanChangedResult());

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(new EntitySavedResult(5));

            Assert.Null(sut.Snapshot.Plan);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanReset())
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(A<string>.Ignored, A<NamedId<Guid>>.Ignored, planIdFree))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_reset_plan_for_reset_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .Returns(new PlanChangedResult());

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdFree))
                .Returns(new PlanChangedResult());

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await PublishIdempotentAsync(command);

            Assert.True(result is PlanChangedResult);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(new RedirectToCheckoutResult(new Uri("http://squidex.io")));

            Assert.Null(sut.Snapshot.Plan);
        }

        [Fact]
        public async Task ChangePlan_should_not_call_billing_manager_for_callback()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(new EntitySavedResult(4));

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task AssignContributor_should_create_events_and_update_state()
        {
            var command = new AssignContributor { ContributorId = contributorId, Role = Role.Editor };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(1, sut.Snapshot.Roles.CustomCount);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

            Assert.Equal(0, sut.Snapshot.Roles.CustomCount);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent2(sut.Snapshot);

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

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent2(new EntitySavedResult(4));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppArchived())
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, AppNamedId, null))
                .MustHaveHappened();
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateApp { Name = AppName });
        }

        private Task ExecuteUploadImage()
        {
            return PublishAsync(new UploadAppImage { File = new AssetFile("image.png", "image/png", 100, () => new MemoryStream()) });
        }

        private Task ExecuteAddPatternAsync()
        {
            return PublishAsync(new AddPattern { PatternId = patternId3, Name = "Name", Pattern = ".*" });
        }

        private Task ExecuteAssignContributorAsync()
        {
            return PublishAsync(new AssignContributor { ContributorId = contributorId, Role = Role.Editor });
        }

        private Task ExecuteAttachClientAsync()
        {
            return PublishAsync(new AttachClient { Id = clientId });
        }

        private Task ExecuteAddRoleAsync()
        {
            return PublishAsync(new AddRole { Name = roleName });
        }

        private Task ExecuteAddLanguageAsync(Language language)
        {
            return PublishAsync(new AddLanguage { Language = language });
        }

        private Task ExecuteAddWorkflowAsync()
        {
            return PublishAsync(new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" });
        }

        private Task ExecuteChangePlanAsync()
        {
            return PublishAsync(new ChangePlan { PlanId = planIdPaid });
        }

        private Task ExecuteArchiveAsync()
        {
            return PublishAsync(new ArchiveApp());
        }

        private async Task<object?> PublishIdempotentAsync(AppCommand command)
        {
            var result = await PublishAsync(command);

            var previousSnapshot = sut.Snapshot;
            var previousVersion = sut.Snapshot.Version;

            await PublishAsync(command);

            Assert.Same(previousSnapshot, sut.Snapshot);
            Assert.Equal(previousVersion, sut.Snapshot.Version);

            return result;
        }

        private async Task<object?> PublishAsync(AppCommand command)
        {
            var result = await sut.ExecuteAsync(CreateCommand(command));

            return result;
        }
    }
}
