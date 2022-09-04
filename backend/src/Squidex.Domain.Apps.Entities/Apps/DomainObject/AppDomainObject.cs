// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Shared.Users;

#pragma warning disable MA0022 // Return Task.FromResult instead of returning null

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public partial class AppDomainObject : DomainObject<AppDomainObject.State>
    {
        private readonly IServiceProvider serviceProvider;

        public AppDomainObject(DomainId id, IPersistenceFactory<State> persistence, ILogger<AppDomainObject> log,
            IServiceProvider serviceProvider)
            : base(id, persistence, log)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override bool IsDeleted(State snapshot)
        {
            return snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateApp;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is AppUpdateCommand update && Equals(update?.AppId?.Id, Snapshot.Id);
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command,
            CancellationToken ct)
        {
            switch (command)
            {
                case CreateApp create:
                    return CreateReturn(create, c =>
                    {
                        GuardApp.CanCreate(c);

                        Create(c);

                        return Snapshot;
                    }, ct);

                case UpdateApp update:
                    return UpdateReturn(update, c =>
                    {
                        GuardApp.CanUpdate(c);

                        Update(c);

                        return Snapshot;
                    }, ct);

                case TransferToTeam transfer:
                    return UpdateReturnAsync(transfer, async (c, ct) =>
                    {
                        await GuardApp.CanTransfer(c, Snapshot, AppProvider(), ct);

                        Transfer(c);

                        return Snapshot;
                    }, ct);

                case UpdateAppSettings updateSettings:
                    return UpdateReturn(updateSettings, c =>
                    {
                        GuardApp.CanUpdateSettings(c);

                        UpdateSettings(c);

                        return Snapshot;
                    }, ct);

                case UploadAppImage uploadImage:
                    return UpdateReturn(uploadImage, c =>
                    {
                        GuardApp.CanUploadImage(c);

                        UploadImage(c);

                        return Snapshot;
                    }, ct);

                case RemoveAppImage removeImage:
                    return UpdateReturn(removeImage, c =>
                    {
                        GuardApp.CanRemoveImage(c);

                        RemoveImage(c);

                        return Snapshot;
                    }, ct);

                case ConfigureAssetScripts configureAssetScripts:
                    return UpdateReturn(configureAssetScripts, c =>
                    {
                        GuardApp.CanUpdateAssetScripts(c);

                        ConfigureAssetScripts(c);

                        return Snapshot;
                    }, ct);

                case AssignContributor assignContributor:
                    return UpdateReturnAsync(assignContributor, async (c, ct) =>
                    {
                        await GuardAppContributors.CanAssign(c, Snapshot, Users(), GetPlan());

                        AssignContributor(c, !Snapshot.Contributors.ContainsKey(assignContributor.ContributorId));

                        return Snapshot;
                    }, ct);

                case RemoveContributor removeContributor:
                    return UpdateReturn(removeContributor, c =>
                    {
                        GuardAppContributors.CanRemove(c, Snapshot);

                        RemoveContributor(c);

                        return Snapshot;
                    }, ct);

                case AttachClient attachClient:
                    return UpdateReturn(attachClient, c =>
                    {
                        GuardAppClients.CanAttach(c, Snapshot);

                        AttachClient(c);

                        return Snapshot;
                    }, ct);

                case UpdateClient updateClient:
                    return UpdateReturn(updateClient, c =>
                    {
                        GuardAppClients.CanUpdate(c, Snapshot);

                        UpdateClient(c);

                        return Snapshot;
                    }, ct);

                case RevokeClient revokeClient:
                    return UpdateReturn(revokeClient, c =>
                    {
                        GuardAppClients.CanRevoke(c, Snapshot);

                        RevokeClient(c);

                        return Snapshot;
                    }, ct);

                case AddWorkflow addWorkflow:
                    return UpdateReturn(addWorkflow, c =>
                    {
                        GuardAppWorkflows.CanAdd(c);

                        AddWorkflow(c);

                        return Snapshot;
                    }, ct);

                case UpdateWorkflow updateWorkflow:
                    return UpdateReturn(updateWorkflow, c =>
                    {
                        GuardAppWorkflows.CanUpdate(c, Snapshot);

                        UpdateWorkflow(c);

                        return Snapshot;
                    }, ct);

                case DeleteWorkflow deleteWorkflow:
                    return UpdateReturn(deleteWorkflow, c =>
                    {
                        GuardAppWorkflows.CanDelete(c, Snapshot);

                        DeleteWorkflow(c);

                        return Snapshot;
                    }, ct);

                case AddLanguage addLanguage:
                    return UpdateReturn(addLanguage, c =>
                    {
                        GuardAppLanguages.CanAdd(c, Snapshot);

                        AddLanguage(c);

                        return Snapshot;
                    }, ct);

                case RemoveLanguage removeLanguage:
                    return UpdateReturn(removeLanguage, c =>
                    {
                        GuardAppLanguages.CanRemove(c, Snapshot);

                        RemoveLanguage(c);

                        return Snapshot;
                    }, ct);

                case UpdateLanguage updateLanguage:
                    return UpdateReturn(updateLanguage, c =>
                    {
                        GuardAppLanguages.CanUpdate(c, Snapshot);

                        UpdateLanguage(c);

                        return Snapshot;
                    }, ct);

                case AddRole addRole:
                    return UpdateReturn(addRole, c =>
                    {
                        GuardAppRoles.CanAdd(c, Snapshot);

                        AddRole(c);

                        return Snapshot;
                    }, ct);

                case DeleteRole deleteRole:
                    return UpdateReturn(deleteRole, c =>
                    {
                        GuardAppRoles.CanDelete(c, Snapshot);

                        DeleteRole(c);

                        return Snapshot;
                    }, ct);

                case UpdateRole updateRole:
                    return UpdateReturn(updateRole, c =>
                    {
                        GuardAppRoles.CanUpdate(c, Snapshot);

                        UpdateRole(c);

                        return Snapshot;
                    }, ct);

                case DeleteApp delete:
                    return UpdateAsync(delete, async (c, ct) =>
                    {
                        await BillingManager().UnsubscribeAsync(c.Actor.Identifier, Snapshot.NamedId(), default);

                        DeleteApp(c);
                    }, ct);

                case ChangePlan changePlan:
                    return ChangeBillingPlanAsync(changePlan, ct);

                default:
                    ThrowHelper.NotSupportedException();
                    return default!;
            }
        }

        private async Task<CommandResult> ChangeBillingPlanAsync(ChangePlan changePlan,
            CancellationToken ct)
        {
            var userId = changePlan.Actor.Identifier;

            var result = await UpdateReturnAsync(changePlan, async (c, ct) =>
            {
                GuardApp.CanChangePlan(c, Snapshot, BillingPlans());

                if (string.Equals(GetFreePlan()?.Id, c.PlanId, StringComparison.Ordinal))
                {
                    ResetPlan(c);

                    return new PlanChangedResult(c.PlanId, true, null);
                }

                if (!c.FromCallback)
                {
                    var redirectUri = await BillingManager().MustRedirectToPortalAsync(userId, Snapshot.NamedId(), c.PlanId, ct);

                    if (redirectUri != null)
                    {
                        return new PlanChangedResult(c.PlanId, false, redirectUri);
                    }
                }

                ChangePlan(c);

                return new PlanChangedResult(c.PlanId);
            }, ct);

            if (changePlan.FromCallback)
            {
                return result;
            }

            if (result.Payload is PlanChangedResult { Unsubscribed: true, RedirectUri: null })
            {
                await BillingManager().UnsubscribeAsync(userId, Snapshot.NamedId(), default);
            }
            else if (result.Payload is PlanChangedResult { RedirectUri: null })
            {
                await BillingManager().SubscribeAsync(userId, Snapshot.NamedId(), changePlan.PlanId, default);
            }

            return result;
        }

        private void Create(CreateApp command)
        {
            var appId = NamedId.Of(command.AppId, command.Name);

            var events = new List<AppEvent>
            {
                CreateInitalEvent(command.Name)
            };

            if (command.Actor.IsUser)
            {
                events.Add(CreateInitialOwner(command.Actor));
            }

            events.Add(CreateInitialSettings());

            foreach (var @event in events)
            {
                @event.AppId = appId;

                Raise(command, @event);
            }
        }

        private void ChangePlan(ChangePlan command)
        {
            Raise(command, new AppPlanChanged());
        }

        private void ResetPlan(ChangePlan command)
        {
            Raise(command, new AppPlanReset());
        }

        private void Update(UpdateApp command)
        {
            Raise(command, new AppUpdated());
        }

        private void Transfer(TransferToTeam command)
        {
            Raise(command, new AppTransfered());
        }

        private void UpdateSettings(UpdateAppSettings command)
        {
            Raise(command, new AppSettingsUpdated());
        }

        private void ConfigureAssetScripts(ConfigureAssetScripts command)
        {
            Raise(command, new AppAssetsScriptsConfigured());
        }

        private void UpdateClient(UpdateClient command)
        {
            Raise(command, new AppClientUpdated());
        }

        private void UploadImage(UploadAppImage command)
        {
            Raise(command, new AppImageUploaded { Image = new AppImage(command.File.MimeType) });
        }

        private void RemoveImage(RemoveAppImage command)
        {
            Raise(command, new AppImageRemoved());
        }

        private void UpdateLanguage(UpdateLanguage command)
        {
            Raise(command, new AppLanguageUpdated());
        }

        private void AssignContributor(AssignContributor command, bool isAdded)
        {
            Raise(command, new AppContributorAssigned { IsAdded = isAdded });
        }

        private void RemoveContributor(RemoveContributor command)
        {
            Raise(command, new AppContributorRemoved());
        }

        private void AttachClient(AttachClient command)
        {
            Raise(command, new AppClientAttached());
        }

        private void RevokeClient(RevokeClient command)
        {
            Raise(command, new AppClientRevoked());
        }

        private void AddWorkflow(AddWorkflow command)
        {
            Raise(command, new AppWorkflowAdded());
        }

        private void UpdateWorkflow(UpdateWorkflow command)
        {
            Raise(command, new AppWorkflowUpdated());
        }

        private void DeleteWorkflow(DeleteWorkflow command)
        {
            Raise(command, new AppWorkflowDeleted());
        }

        private void AddLanguage(AddLanguage command)
        {
            Raise(command, new AppLanguageAdded());
        }

        private void RemoveLanguage(RemoveLanguage command)
        {
            Raise(command, new AppLanguageRemoved());
        }

        private void AddRole(AddRole command)
        {
            Raise(command, new AppRoleAdded());
        }

        private void DeleteRole(DeleteRole command)
        {
            Raise(command, new AppRoleDeleted());
        }

        private void UpdateRole(UpdateRole command)
        {
            Raise(command, new AppRoleUpdated());
        }

        private void DeleteApp(DeleteApp command)
        {
            Raise(command, new AppDeleted());
        }

        private void Raise<T, TEvent>(T command, TEvent @event) where T : class where TEvent : AppEvent
        {
            SimpleMapper.Map(command, @event);

            @event.AppId ??= Snapshot.NamedId();

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppCreated CreateInitalEvent(string name)
        {
            return new AppCreated { Name = name };
        }

        private static AppContributorAssigned CreateInitialOwner(RefToken actor)
        {
            return new AppContributorAssigned { ContributorId = actor.Identifier, Role = Role.Owner };
        }

        private AppSettingsUpdated CreateInitialSettings()
        {
            return new AppSettingsUpdated { Settings = serviceProvider.GetRequiredService<InitialSettings>().Settings };
        }

        private IAppProvider AppProvider()
        {
            return serviceProvider.GetRequiredService<IAppProvider>();
        }

        private IBillingPlans BillingPlans()
        {
            return serviceProvider.GetRequiredService<IBillingPlans>();
        }

        private IBillingManager BillingManager()
        {
            return serviceProvider.GetRequiredService<IBillingManager>();
        }

        private IUserResolver Users()
        {
            return serviceProvider.GetRequiredService<IUserResolver>();
        }

        private Plan GetFreePlan()
        {
            return BillingPlans().GetFreePlan();
        }

        private Plan GetPlan()
        {
            return BillingPlans().GetActualPlan(Snapshot.Plan?.PlanId).Plan;
        }
    }
}
