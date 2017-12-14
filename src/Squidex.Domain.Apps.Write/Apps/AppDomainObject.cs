﻿// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Apps.Utils;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppDomainObject : DomainObjectBase
    {
        private AppContributors contributors = AppContributors.Empty;
        private AppClients clients = AppClients.Empty;
        private LanguagesConfig languagesConfig = LanguagesConfig.English;
        private AppPatterns patterns = AppPatterns.Empty;
        private AppPlan plan;
        private string name;

        public string Name
        {
            get { return name; }
        }

        public AppPlan Plan
        {
            get { return plan; }
        }

        public AppClients Clients
        {
            get { return clients; }
        }

        public AppContributors Contributors
        {
            get { return contributors; }
        }

        public LanguagesConfig LanguagesConfig
        {
            get { return languagesConfig; }
        }

        public AppPatterns Patterns
        {
            get { return patterns; }
        }

        public AppDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(AppCreated @event)
        {
            name = @event.Name;
        }

        protected void On(AppContributorAssigned @event)
        {
            contributors = contributors.Apply(@event);
        }

        protected void On(AppContributorRemoved @event)
        {
            contributors = contributors.Apply(@event);
        }

        protected void On(AppClientAttached @event)
        {
            clients = clients.Apply(@event);
        }

        protected void On(AppClientUpdated @event)
        {
            clients = clients.Apply(@event);
        }

        protected void On(AppClientRenamed @event)
        {
            clients = clients.Apply(@event);
        }

        protected void On(AppClientRevoked @event)
        {
            clients = clients.Apply(@event);
        }

        protected void On(AppLanguageAdded @event)
        {
            languagesConfig = languagesConfig.Apply(@event);
        }

        protected void On(AppLanguageRemoved @event)
        {
            languagesConfig = languagesConfig.Apply(@event);
        }

        protected void On(AppLanguageUpdated @event)
        {
            languagesConfig = languagesConfig.Apply(@event);
        }

        protected void On(AppPlanChanged @event)
        {
            plan = string.IsNullOrWhiteSpace(@event.PlanId) ? null : new AppPlan(@event.Actor, @event.PlanId);
        }

        protected void On(AppPatternAdded @event)
        {
            patterns = patterns.Apply(@event);
        }

        protected void On(AppPatternDeleted @event)
        {
            patterns = patterns.Apply(@event);
        }

        protected void On(AppPatternUpdated @event)
        {
            patterns = patterns.Apply(@event);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            ThrowIfCreated();

            var appId = new NamedId<Guid>(command.AppId, command.Name);

            RaiseEvent(SimpleMapper.Map(command, new AppCreated { AppId = appId }));

            RaiseEvent(SimpleMapper.Map(command, CreateInitialOwner(appId, command)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialLanguage(appId)));

            return this;
        }

        public AppDomainObject UpdateClient(UpdateClient command)
        {
            ThrowIfNotCreated();

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));
            }

            if (command.Permission.HasValue)
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientUpdated { Permission = command.Permission.Value }));
            }

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AddLanguage(AddLanguage command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));

            return this;
        }

        public AppDomainObject RemoveLanguage(RemoveLanguage command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));

            return this;
        }

        public AppDomainObject UpdateLanguage(UpdateLanguage command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageUpdated()));

            return this;
        }

        public AppDomainObject ChangePlan(ChangePlan command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));

            return this;
        }

        public AppDomainObject AddPattern(AddPattern command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternAdded()));

            return this;
        }

        public AppDomainObject DeletePattern(DeletePattern command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternDeleted()));

            return this;
        }

        public AppDomainObject UpdatePattern(UpdatePattern command)
        {
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternUpdated()));

            return this;
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = new NamedId<Guid>(Id, name);
            }

            RaiseEvent(Envelope.Create(@event));
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
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has not been created.");
            }
        }

        private void ThrowIfCreated()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has already been created.");
            }
        }
    }
}
