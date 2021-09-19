// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
{
    public sealed partial class AppDomainObject : DomainObject<AppDomainObject.State>
    {
        private readonly InitialSettings initialSettings;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly IUserResolver userResolver;

        public AppDomainObject(IPersistenceFactory<State> persistence, ISemanticLog log,
            InitialSettings initialPatterns,
            IAppPlansProvider appPlansProvider,
            IAppPlanBillingManager appPlansBillingManager,
            IUserResolver userResolver)
            : base(persistence, log)
        {
            this.userResolver = userResolver;
            this.appPlansProvider = appPlansProvider;
            this.appPlansBillingManager = appPlansBillingManager;
            this.initialSettings = initialPatterns;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsDeleted;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateApp;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is AppUpdateCommand update && Equals(update?.AppId?.Id, Snapshot.Id);
        }

        public override Task<CommandResult> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateApp create:
                    return CreateReturn(create, c =>
                    {
                        GuardApp.CanCreate(c);

                        Create(c);

                        return Snapshot;
                    });

                case UpdateApp update:
                    return UpdateReturn(update, c =>
                    {
                        GuardApp.CanUpdate(c);

                        Update(c);

                        return Snapshot;
                    });

                case UpdateAppSettings updateSettings:
                    return UpdateReturn(updateSettings, c =>
                    {
                        GuardApp.CanUpdateSettings(c);

                        UpdateSettings(c);

                        return Snapshot;
                    });

                case UploadAppImage uploadImage:
                    return UpdateReturn(uploadImage, c =>
                    {
                        GuardApp.CanUploadImage(c);

                        UploadImage(c);

                        return Snapshot;
                    });

                case RemoveAppImage removeImage:
                    return UpdateReturn(removeImage, c =>
                    {
                        GuardApp.CanRemoveImage(c);

                        RemoveImage(c);

                        return Snapshot;
                    });

                case ConfigureAssetScripts configureAssetScripts:
                    return UpdateReturn(configureAssetScripts, c =>
                    {
                        GuardApp.CanUpdateAssetScripts(c);

                        ConfigureAssetScripts(c);

                        return Snapshot;
                    });

                case AssignContributor assignContributor:
                    return UpdateReturnAsync(assignContributor, async c =>
                    {
                        await GuardAppContributors.CanAssign(c, Snapshot, userResolver, GetPlan());

                        AssignContributor(c, !Snapshot.Contributors.ContainsKey(assignContributor.ContributorId));

                        return Snapshot;
                    });

                case RemoveContributor removeContributor:
                    return UpdateReturn(removeContributor, c =>
                    {
                        GuardAppContributors.CanRemove(c, Snapshot);

                        RemoveContributor(c);

                        return Snapshot;
                    });

                case AttachClient attachClient:
                    return UpdateReturn(attachClient, c =>
                    {
                        GuardAppClients.CanAttach(c, Snapshot);

                        AttachClient(c);

                        return Snapshot;
                    });

                case UpdateClient updateClient:
                    return UpdateReturn(updateClient, c =>
                    {
                        GuardAppClients.CanUpdate(c, Snapshot);

                        UpdateClient(c);

                        return Snapshot;
                    });

                case RevokeClient revokeClient:
                    return UpdateReturn(revokeClient, c =>
                    {
                        GuardAppClients.CanRevoke(c, Snapshot);

                        RevokeClient(c);

                        return Snapshot;
                    });

                case AddWorkflow addWorkflow:
                    return UpdateReturn(addWorkflow, c =>
                    {
                        GuardAppWorkflows.CanAdd(c);

                        AddWorkflow(c);

                        return Snapshot;
                    });

                case UpdateWorkflow updateWorkflow:
                    return UpdateReturn(updateWorkflow, c =>
                    {
                        GuardAppWorkflows.CanUpdate(c, Snapshot);

                        UpdateWorkflow(c);

                        return Snapshot;
                    });

                case DeleteWorkflow deleteWorkflow:
                    return UpdateReturn(deleteWorkflow, c =>
                    {
                        GuardAppWorkflows.CanDelete(c, Snapshot);

                        DeleteWorkflow(c);

                        return Snapshot;
                    });

                case AddLanguage addLanguage:
                    return UpdateReturn(addLanguage, c =>
                    {
                        GuardAppLanguages.CanAdd(c, Snapshot);

                        AddLanguage(c);

                        return Snapshot;
                    });

                case RemoveLanguage removeLanguage:
                    return UpdateReturn(removeLanguage, c =>
                    {
                        GuardAppLanguages.CanRemove(c, Snapshot);

                        RemoveLanguage(c);

                        return Snapshot;
                    });

                case UpdateLanguage updateLanguage:
                    return UpdateReturn(updateLanguage, c =>
                    {
                        GuardAppLanguages.CanUpdate(c, Snapshot);

                        UpdateLanguage(c);

                        return Snapshot;
                    });

                case AddRole addRole:
                    return UpdateReturn(addRole, c =>
                    {
                        GuardAppRoles.CanAdd(c, Snapshot);

                        AddRole(c);

                        return Snapshot;
                    });

                case DeleteRole deleteRole:
                    return UpdateReturn(deleteRole, c =>
                    {
                        GuardAppRoles.CanDelete(c, Snapshot);

                        DeleteRole(c);

                        return Snapshot;
                    });

                case UpdateRole updateRole:
                    return UpdateReturn(updateRole, c =>
                    {
                        GuardAppRoles.CanUpdate(c, Snapshot);

                        UpdateRole(c);

                        return Snapshot;
                    });

                case ChangePlan changePlan:
                    return UpdateReturnAsync(changePlan, async c =>
                    {
                        GuardApp.CanChangePlan(c, Snapshot, appPlansProvider);

                        if (c.FromCallback)
                        {
                            ChangePlan(c);

                            return null;
                        }
                        else
                        {
                            var result =
                                await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier,
                                    Snapshot.NamedId(), c.PlanId, c.Referer);

                            switch (result)
                            {
                                case PlanChangedResult:
                                    ChangePlan(c);
                                    break;
                            }

                            return result;
                        }
                    });

                case DeleteApp delete:
                    return UpdateAsync(delete, async c =>
                    {
                        await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), null, null);

                        DeleteApp(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private IAppLimitsPlan GetPlan()
        {
            return appPlansProvider.GetPlanForApp(Snapshot).Plan;
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
            if (string.Equals(appPlansProvider.GetFreePlan()?.Id, command.PlanId, StringComparison.Ordinal))
            {
                Raise(command, new AppPlanReset());
            }
            else
            {
                Raise(command, new AppPlanChanged());
            }
        }

        private void Update(UpdateApp command)
        {
            Raise(command, new AppUpdated());
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
            return new AppSettingsUpdated { Settings = initialSettings.Settings };
        }
    }
}
