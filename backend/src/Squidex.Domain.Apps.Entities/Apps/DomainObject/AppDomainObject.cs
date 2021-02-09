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

                case AddPattern addPattern:
                    return UpdateReturn(addPattern, c =>
                    {
                        GuardAppPatterns.CanAdd(c, Snapshot);

                        AddPattern(c);

                        return Snapshot;
                    });

                case DeletePattern deletePattern:
                    return UpdateReturn(deletePattern, c =>
                    {
                        GuardAppPatterns.CanDelete(c, Snapshot);

                        DeletePattern(c);

                        return Snapshot;
                    });

                case UpdatePattern updatePattern:
                    return UpdateReturn(updatePattern, c =>
                    {
                        GuardAppPatterns.CanUpdate(c, Snapshot);

                        UpdatePattern(c);

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
                            var result = await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), c.PlanId, c.Referer);

                            switch (result)
                            {
                                case PlanChangedResult:
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

            if (command.Actor.IsUser)
            {
                events.Add(CreateInitialOwner(command.Actor));
            }

            foreach (var (key, value) in initialPatterns)
            {
                events.Add(CreateInitialPattern(key, value));
            }

            foreach (var @event in events)
            {
                @event.AppId = appId;

                Raise(command, @event);
            }
        }

        public void ChangePlan(ChangePlan command)
        {
            if (string.Equals(appPlansProvider.GetFreePlan()?.Id, command.PlanId))
            {
                Raise(command, new AppPlanReset());
            }
            else
            {
                Raise(command, new AppPlanChanged());
            }
        }

        public void Update(UpdateApp command)
        {
            Raise(command, new AppUpdated());
        }

        public void UpdateClient(UpdateClient command)
        {
            Raise(command, new AppClientUpdated());
        }

        public void UploadImage(UploadAppImage command)
        {
            Raise(command, new AppImageUploaded { Image = new AppImage(command.File.MimeType) });
        }

        public void RemoveImage(RemoveAppImage command)
        {
            Raise(command, new AppImageRemoved());
        }

        public void UpdateLanguage(UpdateLanguage command)
        {
            Raise(command, new AppLanguageUpdated());
        }

        public void AssignContributor(AssignContributor command, bool isAdded)
        {
            Raise(command, new AppContributorAssigned { IsAdded = isAdded });
        }

        public void RemoveContributor(RemoveContributor command)
        {
            Raise(command, new AppContributorRemoved());
        }

        public void AttachClient(AttachClient command)
        {
            Raise(command, new AppClientAttached());
        }

        public void RevokeClient(RevokeClient command)
        {
            Raise(command, new AppClientRevoked());
        }

        public void AddWorkflow(AddWorkflow command)
        {
            Raise(command, new AppWorkflowAdded());
        }

        public void UpdateWorkflow(UpdateWorkflow command)
        {
            Raise(command, new AppWorkflowUpdated());
        }

        public void DeleteWorkflow(DeleteWorkflow command)
        {
            Raise(command, new AppWorkflowDeleted());
        }

        public void AddLanguage(AddLanguage command)
        {
            Raise(command, new AppLanguageAdded());
        }

        public void RemoveLanguage(RemoveLanguage command)
        {
            Raise(command, new AppLanguageRemoved());
        }

        public void AddPattern(AddPattern command)
        {
            Raise(command, new AppPatternAdded());
        }

        public void DeletePattern(DeletePattern command)
        {
            Raise(command, new AppPatternDeleted());
        }

        public void UpdatePattern(UpdatePattern command)
        {
            Raise(command, new AppPatternUpdated());
        }

        public void AddRole(AddRole command)
        {
            Raise(command, new AppRoleAdded());
        }

        public void DeleteRole(DeleteRole command)
        {
            Raise(command, new AppRoleDeleted());
        }

        public void UpdateRole(UpdateRole command)
        {
            Raise(command, new AppRoleUpdated());
        }

        public void ArchiveApp(ArchiveApp command)
        {
            Raise(command, new AppArchived());
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
