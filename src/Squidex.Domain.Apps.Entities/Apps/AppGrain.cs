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
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppGrain : SquidexDomainObjectGrain<AppState>, IAppGrain
    {
        private readonly InitialPatterns initialPatterns;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly IUserResolver userResolver;

        public AppGrain(
            InitialPatterns initialPatterns,
            IStore<Guid> store,
            ISemanticLog log,
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

        protected override Task<object> ExecuteAsync(IAggregateCommand command)
        {
            VerifyNotArchived();

            switch (command)
            {
                case CreateApp createApp:
                    return CreateReturnAsync(createApp, async c =>
                    {
                        GuardApp.CanCreate(c);

                        Create(c);

                        return await GetRawStateAsync();
                    });

                case AssignContributor assignContributor:
                    return UpdateReturnAsync(assignContributor, async c =>
                    {
                        await GuardAppContributors.CanAssign(Snapshot.Contributors, Snapshot.Roles, c, userResolver, GetPlan());

                        AssignContributor(c, !Snapshot.Contributors.ContainsKey(assignContributor.ContributorId));

                        return await GetRawStateAsync();
                    });

                case RemoveContributor removeContributor:
                    return UpdateReturnAsync(removeContributor, async c =>
                    {
                        GuardAppContributors.CanRemove(Snapshot.Contributors, c);

                        RemoveContributor(c);

                        return await GetRawStateAsync();
                    });

                case AttachClient attachClient:
                    return UpdateReturnAsync(attachClient, async c =>
                    {
                        GuardAppClients.CanAttach(Snapshot.Clients, c);

                        AttachClient(c);

                        return await GetRawStateAsync();
                    });

                case UpdateClient updateClient:
                    return UpdateReturnAsync(updateClient, async c =>
                    {
                        GuardAppClients.CanUpdate(Snapshot.Clients, c, Snapshot.Roles);

                        UpdateClient(c);

                        return await GetRawStateAsync();
                    });

                case RevokeClient revokeClient:
                    return UpdateReturnAsync(revokeClient, async c =>
                    {
                        GuardAppClients.CanRevoke(Snapshot.Clients, c);

                        RevokeClient(c);

                        return await GetRawStateAsync();
                    });

                case AddLanguage addLanguage:
                    return UpdateReturnAsync(addLanguage, async c =>
                    {
                        GuardAppLanguages.CanAdd(Snapshot.LanguagesConfig, c);

                        AddLanguage(c);

                        return await GetRawStateAsync();
                    });

                case RemoveLanguage removeLanguage:
                    return UpdateReturnAsync(removeLanguage, async c =>
                    {
                        GuardAppLanguages.CanRemove(Snapshot.LanguagesConfig, c);

                        RemoveLanguage(c);

                        return await GetRawStateAsync();
                    });

                case UpdateLanguage updateLanguage:
                    return UpdateReturnAsync(updateLanguage, async c =>
                    {
                        GuardAppLanguages.CanUpdate(Snapshot.LanguagesConfig, c);

                        UpdateLanguage(c);

                        return await GetRawStateAsync();
                    });

                case AddRole addRole:
                    return UpdateReturnAsync(addRole, async c =>
                    {
                        GuardAppRoles.CanAdd(Snapshot.Roles, c);

                        AddRole(c);

                        return await GetRawStateAsync();
                    });

                case DeleteRole deleteRole:
                    return UpdateReturnAsync(deleteRole, async c =>
                    {
                        GuardAppRoles.CanDelete(Snapshot.Roles, c, Snapshot.Contributors, Snapshot.Clients);

                        DeleteRole(c);

                        return await GetRawStateAsync();
                    });

                case UpdateRole updateRole:
                    return UpdateReturnAsync(updateRole, async c =>
                    {
                        GuardAppRoles.CanUpdate(Snapshot.Roles, c);

                        UpdateRole(c);

                        return await GetRawStateAsync();
                    });

                case AddPattern addPattern:
                    return UpdateReturnAsync(addPattern, async c =>
                    {
                        GuardAppPatterns.CanAdd(Snapshot.Patterns, c);

                        AddPattern(c);

                        return await GetRawStateAsync();
                    });

                case DeletePattern deletePattern:
                    return UpdateReturnAsync(deletePattern, async c =>
                    {
                        GuardAppPatterns.CanDelete(Snapshot.Patterns, c);

                        DeletePattern(c);

                        return await GetRawStateAsync();
                    });

                case UpdatePattern updatePattern:
                    return UpdateReturnAsync(updatePattern, async c =>
                    {
                        GuardAppPatterns.CanUpdate(Snapshot.Patterns, c);

                        UpdatePattern(c);

                        return await GetRawStateAsync();
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
                            var result = await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), c.PlanId);

                            switch (result)
                            {
                                case PlanChangedResult _:
                                    ChangePlan(c);
                                    break;
                                case PlanResetResult _:
                                    ResetPlan(c);
                                    break;
                            }

                            return result;
                        }
                    });

                case ArchiveApp archiveApp:
                    return UpdateAsync(archiveApp, async c =>
                    {
                        await appPlansBillingManager.ChangePlanAsync(c.Actor.Identifier, Snapshot.NamedId(), null);

                        ArchiveApp(c);
                    });

                default:
                    throw new NotSupportedException();
            }
        }

        private IAppLimitsPlan GetPlan()
        {
            return appPlansProvider.GetPlan(Snapshot.Plan?.PlanId);
        }

        public void Create(CreateApp command)
        {
            var appId = NamedId.Of(command.AppId, command.Name);

            var events = new List<AppEvent>
            {
                CreateInitalEvent(command.Name),
                CreateInitialOwner(command.Actor),
                CreateInitialLanguage()
            };

            foreach (var pattern in initialPatterns)
            {
                events.Add(CreateInitialPattern(pattern.Key, pattern.Value));
            }

            foreach (var @event in events)
            {
                @event.Actor = command.Actor;
                @event.AppId = appId;

                RaiseEvent(@event);
            }
        }

        public void UpdateClient(UpdateClient command)
        {
            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));
            }

            if (command.Role != null)
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientUpdated { Role = command.Role }));
            }
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

        public void AddLanguage(AddLanguage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));
        }

        public void RemoveLanguage(RemoveLanguage command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));
        }

        public void ChangePlan(ChangePlan command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));
        }

        public void ResetPlan(ChangePlan command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppPlanReset()));
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

        private void VerifyNotArchived()
        {
            if (Snapshot.IsArchived)
            {
                throw new DomainException("App has already been archived.");
            }
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = NamedId.Of(Snapshot.Id, Snapshot.Name);
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppCreated CreateInitalEvent(string name)
        {
            return new AppCreated { Name = name };
        }

        private static AppPatternAdded CreateInitialPattern(Guid id, AppPattern pattern)
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

        public Task<IAppEntity> GetRawStateAsync()
        {
            return Task.FromResult<IAppEntity>(Snapshot);
        }

        public Task<J<IAppEntity>> GetStateAsync()
        {
            return J.AsTask<IAppEntity>(Snapshot);
        }
    }
}
