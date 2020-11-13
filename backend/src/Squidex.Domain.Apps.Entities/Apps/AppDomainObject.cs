// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Guards;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppDomainObject : DomainObject<AppState>
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly IUserResolver userResolver;

        public AppDomainObject(IStore<DomainId> store, ISemanticLog log,
            InitialPatterns initialPatterns,
            IAppPlansProvider appPlansProvider,
            IAppPlanBillingManager appPlansBillingManager,
            IUserResolver userResolver)
            : base(store, log)
        {
            Guard.NotNull(initialPatterns, nameof(initialPatterns));
            Guard.NotNull(userResolver, nameof(userResolver));
            Guard.NotNull(appPlansProvider, nameof(appPlansProvider));
            Guard.NotNull(appPlansBillingManager, nameof(appPlansBillingManager));

            this.userResolver = userResolver;
            this.appPlansProvider = appPlansProvider;
            this.appPlansBillingManager = appPlansBillingManager;
            this.initialPatterns = initialPatterns;
        }

        protected override bool IsDeleted()
        {
            return Snapshot.IsArchived;
        }

        protected override bool CanAcceptCreation(ICommand command)
        {
            return command is CreateApp;
        }

        protected override bool CanAccept(ICommand command)
        {
            return command is AppUpdateCommand update && Equals(update?.AppId?.Id, Snapshot.Id);
        }

        public override Task<object?> ExecuteAsync(IAggregateCommand command)
        {
            switch (command)
            {
                case CreateApp createApp:
                    return CreateReturn(createApp, c =>
                    {
                        GuardApp.CanCreate(c);

                        Create(c);

                        return Snapshot;
                    });

                case UpdateApp updateApp:
                    return UpdateReturn(updateApp, c =>
                    {
                        GuardApp.CanUpdate(c);

                        Update(c);

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

                case AssignContributor assignContributor:
                    return UpdateReturnAsync(assignContributor, async c =>
                    {
                        await GuardAppContributors.CanAssign(Snapshot.Contributors, Snapshot.Roles, c, userResolver, GetPlan());

                        AssignContributor(c, !Snapshot.Contributors.ContainsKey(assignContributor.ContributorId));

                        return Snapshot;
                    });

                case RemoveContributor removeContributor:
                    return UpdateReturn(removeContributor, c =>
                    {
                        GuardAppContributors.CanRemove(Snapshot.Contributors, c);

                        RemoveContributor(c);

                        return Snapshot;
                    });

                case AttachClient attachClient:
                    return UpdateReturn(attachClient, c =>
                    {
                        GuardAppClients.CanAttach(Snapshot.Clients, c);

                        AttachClient(c);

                        return Snapshot;
                    });

                case UpdateClient updateClient:
                    return UpdateReturn(updateClient, c =>
                    {
                        GuardAppClients.CanUpdate(Snapshot.Clients, c, Snapshot.Roles);

                        UpdateClient(c);

                        return Snapshot;
                    });

                case RevokeClient revokeClient:
                    return UpdateReturn(revokeClient, c =>
                    {
                        GuardAppClients.CanRevoke(Snapshot.Clients, c);

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
                        GuardAppWorkflows.CanUpdate(Snapshot.Workflows, c);

                        UpdateWorkflow(c);

                        return Snapshot;
                    });

                case DeleteWorkflow deleteWorkflow:
                    return UpdateReturn(deleteWorkflow, c =>
                    {
                        GuardAppWorkflows.CanDelete(Snapshot.Workflows, c);

                        DeleteWorkflow(c);

                        return Snapshot;
                    });

                case AddLanguage addLanguage:
                    return UpdateReturn(addLanguage, c =>
                    {
                        GuardAppLanguages.CanAdd(Snapshot.Languages, c);

                        AddLanguage(c);

                        return Snapshot;
                    });

                case RemoveLanguage removeLanguage:
                    return UpdateReturn(removeLanguage, c =>
                    {
                        GuardAppLanguages.CanRemove(Snapshot.Languages, c);

                        RemoveLanguage(c);

                        return Snapshot;
                    });

                case UpdateLanguage updateLanguage:
                    return UpdateReturn(updateLanguage, c =>
                    {
                        GuardAppLanguages.CanUpdate(Snapshot.Languages, c);

                        UpdateLanguage(c);

                        return Snapshot;
                    });

                case AddRole addRole:
                    return UpdateReturn(addRole, c =>
                    {
                        GuardAppRoles.CanAdd(Snapshot.Roles, c);

                        AddRole(c);

                        return Snapshot;
                    });

                case DeleteRole deleteRole:
                    return UpdateReturn(deleteRole, c =>
                    {
                        GuardAppRoles.CanDelete(Snapshot.Roles, c, Snapshot.Contributors, Snapshot.Clients);

                        DeleteRole(c);

                        return Snapshot;
                    });

                case UpdateRole updateRole:
                    return UpdateReturn(updateRole, c =>
                    {
                        GuardAppRoles.CanUpdate(Snapshot.Roles, c);

                        UpdateRole(c);

                        return Snapshot;
                    });

                case AddPattern addPattern:
                    return UpdateReturn(addPattern, c =>
                    {
                        GuardAppPatterns.CanAdd(Snapshot.Patterns, c);

                        AddPattern(c);

                        return Snapshot;
                    });

                case DeletePattern deletePattern:
                    return UpdateReturn(deletePattern, c =>
                    {
                        GuardAppPatterns.CanDelete(Snapshot.Patterns, c);

                        DeletePattern(c);

                        return Snapshot;
                    });

                case UpdatePattern updatePattern:
                    return UpdateReturn(updatePattern, c =>
                    {
                        GuardAppPatterns.CanUpdate(Snapshot.Patterns, c);

                        UpdatePattern(c);

                        return Snapshot;
                    });

                case ChangePlan changePlan:
                    return UpdateReturnAsync(changePlan, async c =>
                    {
                        GuardApp.CanChangePlan(c, Snapshot.Plan, appPlansProvider);

                        if (c.FromCallback)
                        {
                            ChangePlan(c);

                            return null;
                        }
                        else
                        {
                            var result = await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), c.PlanId, c.Referer);

                            switch (result)
                            {
                                case PlanChangedResult _:
                                    ChangePlan(c);
                                    break;
                            }

                            return result;
                        }
                    });

                case ArchiveApp archiveApp:
                    return UpdateAsync(archiveApp, async c =>
                    {
                        await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), null, null);

                        ArchiveApp(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private IAppLimitsPlan GetPlan()
        {
            return appPlansProvider.GetPlanForApp(Snapshot).Plan;
        }

        public void Create(CreateApp command)
        {
            var appId = NamedId.Of(command.AppId, command.Name);

            var events = new List<AppEvent>
            {
                CreateInitalEvent(command.Name),
                CreateInitialLanguage()
            };

            if (command.Actor.IsSubject)
            {
                events.Add(CreateInitialOwner(command.Actor));
            }

            foreach (var (key, value) in initialPatterns)
            {
                events.Add(CreateInitialPattern(key, value));
            }

            foreach (var @event in events)
            {
                @event.Actor = command.Actor;
                @event.AppId = appId;

                RaiseEvent(@event);
            }
        }

        public void ChangePlan(ChangePlan command)
        {
            if (string.Equals(appPlansProvider.GetFreePlan()?.Id, command.PlanId))
            {
                RaiseEvent(SimpleMapper.Map(command, new AppPlanReset()));
            }
            else
            {
                RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));
            }
        }

        public void Update(UpdateApp command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppUpdated()));
        }

        public void UpdateClient(UpdateClient command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppClientUpdated()));
        }

        public void UploadImage(UploadAppImage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppImageUploaded { Image = new AppImage(command.File.MimeType) }));
        }

        public void RemoveImage(RemoveAppImage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppImageRemoved()));
        }

        public void UpdateLanguage(UpdateLanguage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppLanguageUpdated()));
        }

        public void AssignContributor(AssignContributor command, bool isAdded)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned { IsAdded = isAdded }));
        }

        public void RemoveContributor(RemoveContributor command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));
        }

        public void AttachClient(AttachClient command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached()));
        }

        public void RevokeClient(RevokeClient command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));
        }

        public void AddWorkflow(AddWorkflow command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppWorkflowAdded()));
        }

        public void UpdateWorkflow(UpdateWorkflow command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppWorkflowUpdated()));
        }

        public void DeleteWorkflow(DeleteWorkflow command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppWorkflowDeleted()));
        }

        public void AddLanguage(AddLanguage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));
        }

        public void RemoveLanguage(RemoveLanguage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));
        }

        public void AddPattern(AddPattern command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppPatternAdded()));
        }

        public void DeletePattern(DeletePattern command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppPatternDeleted()));
        }

        public void UpdatePattern(UpdatePattern command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppPatternUpdated()));
        }

        public void AddRole(AddRole command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppRoleAdded()));
        }

        public void DeleteRole(DeleteRole command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppRoleDeleted()));
        }

        public void UpdateRole(UpdateRole command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppRoleUpdated()));
        }

        public void ArchiveApp(ArchiveApp command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppArchived()));
        }

        private void RaiseEvent(AppEvent @event)
        {
            @event.AppId ??= Snapshot.NamedId();

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppCreated CreateInitalEvent(string name)
        {
            return new AppCreated { Name = name };
        }

        private static AppPatternAdded CreateInitialPattern(DomainId id, AppPattern pattern)
        {
            return new AppPatternAdded { PatternId = id, Name = pattern.Name, Pattern = pattern.Pattern, Message = pattern.Message };
        }

        private static AppLanguageAdded CreateInitialLanguage()
        {
            return new AppLanguageAdded { Language = Language.EN };
        }

        private static AppContributorAssigned CreateInitialOwner(RefToken actor)
        {
            return new AppContributorAssigned { ContributorId = actor.Identifier, Role = Role.Owner };
        }
    }
}
