// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppDomainObject : DomainObjectBase<AppDomainObject, AppState>
    {
        public AppDomainObject Create(CreateApp command)
        {
            ThrowIfCreated();

            var appId = new NamedId<Guid>(command.AppId, command.Name);

            UpdateState(command, s => { s.Id = appId.Id; s.Name = command.Name; });

            UpdateContributors(command, c => c.Assign(command.Actor.Identifier, AppContributorPermission.Owner));

            RaiseEvent(SimpleMapper.Map(command, CreateInitalEvent(appId)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialOwner(appId, command)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialLanguage(appId)));

            return this;
        }

        public AppDomainObject UpdateLanguage(UpdateLanguage command)
        {
            ThrowIfNotCreated();

            UpdateLanguages(command, l =>
            {
                var fallback = command.Fallback;

                if (fallback != null && fallback.Count > 0)
                {
                    var existingLangauges = l.OfType<LanguageConfig>().Select(x => x.Language);

                    fallback = fallback.Intersect(existingLangauges).ToList();
                }

                l = l.Set(new LanguageConfig(command.Language, command.IsOptional, fallback));

                if (command.IsMaster)
                {
                    l = l.MakeMaster(command.Language);
                }

                return l;
            });

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageUpdated()));

            return this;
        }

        public AppDomainObject UpdateClient(UpdateClient command)
        {
            ThrowIfNotCreated();

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                UpdateClients(command, c => c.Rename(command.Id, command.Name));

                RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));
            }

            if (command.Permission.HasValue)
            {
                UpdateClients(command, c => c.Update(command.Id, command.Permission.Value));

                RaiseEvent(SimpleMapper.Map(command, new AppClientUpdated { Permission = command.Permission.Value }));
            }

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            ThrowIfNotCreated();

            UpdateContributors(command, c => c.Assign(command.ContributorId, command.Permission));

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            ThrowIfNotCreated();

            UpdateContributors(command, c => c.Remove(command.ContributorId));

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command)
        {
            ThrowIfNotCreated();

            UpdateClients(command, c => c.Add(command.Id, command.Secret));

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            ThrowIfNotCreated();

            UpdateClients(command, c => c.Revoke(command.Id));

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AddLanguage(AddLanguage command)
        {
            ThrowIfNotCreated();

            UpdateLanguages(command, l => l.Set(new LanguageConfig(command.Language)));

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));

            return this;
        }

        public AppDomainObject RemoveLanguage(RemoveLanguage command)
        {
            ThrowIfNotCreated();

            UpdateLanguages(command, l => l.Remove(command.Language));

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));

            return this;
        }

        public AppDomainObject ChangePlan(ChangePlan command)
        {
            ThrowIfNotCreated();

            UpdateState(command, s => s.Plan = command.PlanId != null ? new AppPlan(command.Actor, command.PlanId) : null);

            RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));

            return this;
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = new NamedId<Guid>(State.Id, State.Name);
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppCreated CreateInitalEvent(NamedId<Guid> appId)
        {
            return new AppCreated { AppId = appId };
        }

        private static AppLanguageAdded CreateInitialLanguage(NamedId<Guid> id)
        {
            return new AppLanguageAdded { AppId = id, Language = Language.EN };
        }

        private static AppContributorAssigned CreateInitialOwner(NamedId<Guid> id, SquidexCommand command)
        {
            return new AppContributorAssigned { AppId = id, ContributorId = command.Actor.Identifier, Permission = AppContributorPermission.Owner };
        }

        private void ThrowIfNotCreated()
        {
            if (string.IsNullOrWhiteSpace(State.Name))
            {
                throw new DomainException("App has not been created.");
            }
        }

        private void ThrowIfCreated()
        {
            if (!string.IsNullOrWhiteSpace(State.Name))
            {
                throw new DomainException("App has already been created.");
            }
        }

        private void UpdateClients(ICommand command, Func<AppClients, AppClients> updater)
        {
            UpdateState(command, s => s.Clients = updater(s.Clients));
        }

        private void UpdateContributors(ICommand command, Func<AppContributors, AppContributors> updater)
        {
            UpdateState(command, s => s.Contributors = updater(s.Contributors));
        }

        private void UpdateLanguages(ICommand command, Func<LanguagesConfig, LanguagesConfig> updater)
        {
            UpdateState(command, s => s.LanguagesConfig = updater(s.LanguagesConfig));
        }

        protected override AppState CloneState(ICommand command, Action<AppState> updater)
        {
            return State.Clone().Update((SquidexCommand)command, updater);
        }
    }
}
