// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public class AppDomainObjectTests : HandlerTestBase<AppDomainObject.State>
    {
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IAppPlanBillingManager appPlansBillingManager = A.Fake<IAppPlanBillingManager>();
        private readonly IUser user;
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly string contributorId = DomainId.NewGuid().ToString();
        private readonly string clientId = "client";
        private readonly string clientNewName = "My Client";
        private readonly string roleName = "My Role";
        private readonly string planIdPaid = "premium";
        private readonly string planIdFree = "free";
        private readonly AppDomainObject sut;
        private readonly DomainId workflowId = DomainId.NewGuid();
        private readonly InitialSettings initialSettings;

        protected override DomainId Id
        {
            get => AppId;
        }

        public AppDomainObjectTests()
        {
            user = UserMocks.User(contributorId);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(contributorId, default))
                .Returns(user);

            A.CallTo(() => appPlansProvider.GetFreePlan())
                .Returns(new ConfigAppLimitsPlan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => appPlansProvider.GetPlan(planIdFree))
                .Returns(new ConfigAppLimitsPlan { Id = planIdFree, MaxContributors = 10 });

            A.CallTo(() => appPlansProvider.GetPlan(planIdPaid))
                .Returns(new ConfigAppLimitsPlan { Id = planIdPaid, MaxContributors = 30 });

            // Create a non-empty setting, otherwise the event is not raised as it does not change the domain object.
            initialSettings = new InitialSettings
            {
                Settings = new AppSettings
                {
                    HideScheduler = true
                }
            };

            sut = new AppDomainObject(PersistenceFactory, A.Dummy<ISemanticLog>(), initialSettings, appPlansProvider, appPlansBillingManager, userResolver);
#pragma warning disable MA0056 // Do not call overridable members in constructor
            sut.Setup(Id);
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
            var command = new CreateApp { Name = AppName, AppId = AppId };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(AppName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }),
                    CreateEvent(new AppContributorAssigned { ContributorId = Actor.Identifier, Role = Role.Owner }),
                    CreateEvent(new AppSettingsUpdated { Settings = initialSettings.Settings })
                );
        }

        [Fact]
        public async Task Create_should_not_assign_client_as_contributor()
        {
            var command = new CreateApp { Name = AppName, Actor = ActorClient, AppId = AppId };

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(AppName, sut.Snapshot.Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppCreated { Name = AppName }, true), // Must be with client actor.
                    CreateEvent(new AppSettingsUpdated { Settings = initialSettings.Settings }, true)
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_label_and_description()
        {
            var command = new UpdateApp { Label = "my-label", Description = "my-description" };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal("my-label", sut.Snapshot.Label);
            Assert.Equal("my-description", sut.Snapshot.Description);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppUpdated { Label = command.Label, Description = command.Description })
                );
        }

        [Fact]
        public async Task UpdateSettings_should_create_event_and_update_settings()
        {
            var settings = new AppSettings
            {
                HideDateTimeModeButton = true
            };

            var command = new UpdateAppSettings { Settings = settings };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(settings, sut.Snapshot.Settings);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppSettingsUpdated { Settings = settings })
                );
        }

        [Fact]
        public async Task UploadImage_should_create_events_and_update_image()
        {
            var command = new UploadAppImage { File = new NoopAssetFile() };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal("image/png", sut.Snapshot.Image!.MimeType);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppImageUploaded { Image = sut.Snapshot.Image })
                );
        }

        [Fact]
        public async Task RemoveImage_should_create_events_and_update_image()
        {
            var command = new RemoveAppImage();

            await ExecuteCreateAsync();
            await ExecuteUploadImage();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Null(sut.Snapshot.Image);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppImageRemoved())
                );
        }

        [Fact]
        public async Task ChangePlan_should_create_events_and_update_plan()
        {
            var command = new ChangePlan { PlanId = planIdPaid };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid, command.Referer))
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
        public async Task ChangePlan_from_callback_should_create_events_and_update_plan()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.Equal(planIdPaid, sut.Snapshot.Plan!.PlanId);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanChanged { PlanId = planIdPaid })
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(A<string>._, A<NamedId<DomainId>>._, A<string?>._, A<string?>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_from_callback_should_reset_plan_for_free_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree, FromCallback = true };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid, command.Referer))
                .Returns(new PlanChangedResult());

            await ExecuteCreateAsync();
            await ExecuteChangePlanAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.Null(sut.Snapshot.Plan);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppPlanReset())
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(A<string>._, A<NamedId<DomainId>>._, planIdFree, A<string?>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ChangePlan_should_reset_plan_for_free_plan()
        {
            var command = new ChangePlan { PlanId = planIdFree };

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid, command.Referer))
                .Returns(new PlanChangedResult());

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdFree, command.Referer))
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

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid, command.Referer))
                .Returns(new RedirectToCheckoutResult(new Uri("http://squidex.io")));

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(new RedirectToCheckoutResult(new Uri("http://squidex.io")));

            Assert.Null(sut.Snapshot.Plan);
        }

        [Fact]
        public async Task ChangePlan_should_not_call_billing_manager_for_callback()
        {
            var command = new ChangePlan { PlanId = planIdPaid, FromCallback = true };

            await ExecuteCreateAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(None.Value);

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(Actor.Identifier, AppNamedId, planIdPaid, A<string?>._))
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
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Role = Role.Editor, IsAdded = true })
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
                    CreateEvent(new AppContributorAssigned { ContributorId = contributorId, Role = Role.Owner })
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
                    CreateEvent(new AppContributorRemoved { ContributorId = contributorId })
                );
        }

        [Fact]
        public async Task AttachClient_should_create_events_and_add_client()
        {
            var command = new AttachClient { Id = clientId };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.Clients.ContainsKey(clientId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientAttached { Id = clientId, Secret = command.Secret })
                );
        }

        [Fact]
        public async Task UpdateClient_should_create_events_and_update_client()
        {
            var command = new UpdateClient { Id = clientId, Name = clientNewName, Role = Role.Developer };

            await ExecuteCreateAsync();
            await ExecuteAttachClientAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(clientNewName, sut.Snapshot.Clients[clientId].Name);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientUpdated { Id = clientId, Name = clientNewName, Role = Role.Developer })
                );
        }

        [Fact]
        public async Task RevokeClient_should_create_events_and_remove_client()
        {
            var command = new RevokeClient { Id = clientId };

            await ExecuteCreateAsync();
            await ExecuteAttachClientAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.Clients.ContainsKey(clientId));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppClientRevoked { Id = clientId })
                );
        }

        [Fact]
        public async Task AddWorkflow_should_create_events_and_add_workflow()
        {
            var command = new AddWorkflow { WorkflowId = workflowId, Name = "my-workflow" };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEmpty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowAdded { WorkflowId = workflowId, Name = "my-workflow" })
                );
        }

        [Fact]
        public async Task UpdateWorkflow_should_create_events_and_update_workflow()
        {
            var command = new UpdateWorkflow { WorkflowId = workflowId, Workflow = Workflow.Default };

            await ExecuteCreateAsync();
            await ExecuteAddWorkflowAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.NotEmpty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowUpdated { WorkflowId = workflowId, Workflow = Workflow.Default })
                );
        }

        [Fact]
        public async Task DeleteWorkflow_should_create_events_and_remove_workflow()
        {
            var command = new DeleteWorkflow { WorkflowId = workflowId };

            await ExecuteCreateAsync();
            await ExecuteAddWorkflowAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Empty(sut.Snapshot.Workflows);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppWorkflowDeleted { WorkflowId = workflowId })
                );
        }

        [Fact]
        public async Task AddLanguage_should_create_events_and_add_language()
        {
            var command = new AddLanguage { Language = Language.DE };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.Languages.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageAdded { Language = Language.DE })
                );
        }

        [Fact]
        public async Task RemoveLanguage_should_create_events_and_remove_language()
        {
            var command = new RemoveLanguage { Language = Language.DE };

            await ExecuteCreateAsync();
            await ExecuteAddLanguageAsync(Language.DE);

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.False(sut.Snapshot.Languages.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageRemoved { Language = Language.DE })
                );
        }

        [Fact]
        public async Task UpdateLanguage_should_create_events_and_update_language()
        {
            var command = new UpdateLanguage { Language = Language.DE, Fallback = new[] { Language.EN } };

            await ExecuteCreateAsync();
            await ExecuteAddLanguageAsync(Language.DE);

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.True(sut.Snapshot.Languages.Contains(Language.DE));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppLanguageUpdated { Language = Language.DE, Fallback = new[] { Language.EN } })
                );
        }

        [Fact]
        public async Task AddRole_should_create_events_and_add_role()
        {
            var command = new AddRole { Name = roleName };

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(1, sut.Snapshot.Roles.CustomCount);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleAdded { Name = roleName })
                );
        }

        [Fact]
        public async Task DeleteRole_should_create_events_and_delete_role()
        {
            var command = new DeleteRole { Name = roleName };

            await ExecuteCreateAsync();
            await ExecuteAddRoleAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            Assert.Equal(0, sut.Snapshot.Roles.CustomCount);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleDeleted { Name = roleName })
                );
        }

        [Fact]
        public async Task UpdateRole_should_create_events_and_update_role()
        {
            var command = new UpdateRole { Name = roleName, Permissions = new[] { "clients.read" }, Properties = JsonValue.Object() };

            await ExecuteCreateAsync();
            await ExecuteAddRoleAsync();

            var result = await PublishIdempotentAsync(command);

            result.ShouldBeEquivalent(sut.Snapshot);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppRoleUpdated { Name = roleName, Permissions = command.Permissions, Properties = command.Properties })
                );
        }

        [Fact]
        public async Task ArchiveApp_should_create_events_and_update_deleted_flag()
        {
            var command = new DeleteApp();

            await ExecuteCreateAsync();

            var result = await PublishAsync(command);

            result.ShouldBeEquivalent(None.Value);

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateEvent(new AppDeleted())
                );

            A.CallTo(() => appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, AppNamedId, null, A<string?>._))
                .MustHaveHappened();
        }

        private Task ExecuteCreateAsync()
        {
            return PublishAsync(new CreateApp { Name = AppName, AppId = AppId });
        }

        private Task ExecuteUploadImage()
        {
            return PublishAsync(new UploadAppImage { File = new NoopAssetFile() });
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
            return PublishAsync(new DeleteApp());
        }

        private Task<object> PublishIdempotentAsync(AppCommand command)
        {
            return PublishIdempotentAsync(sut, CreateCommand(command));
        }

        private async Task<object> PublishAsync(AppCommand command)
        {
            var result = await sut.ExecuteAsync(CreateCommand(command));

            return result.Payload;
        }
    }
}
