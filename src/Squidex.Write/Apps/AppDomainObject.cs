// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Core.Apps;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Write.Apps.Commands;

// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public class AppDomainObject : DomainObject
    {
        private static readonly Language DefaultLanguage = Language.EN;
        private readonly AppContributors contributors = new AppContributors();
        private readonly AppLanguages languages = new AppLanguages();
        private readonly AppClients clients = new AppClients();
        private string name;

        public string Name
        {
            get { return name; }
        }

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients.Clients; }
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
            contributors.Assign(@event.ContributorId, @event.Permission);
        }

        protected void On(AppContributorRemoved @event)
        {
            contributors.Remove(@event.ContributorId);
        }

        protected void On(AppClientAttached @event)
        {
            clients.Add(@event.Id, @event.Secret, @event.ExpiresUtc);
        }

        protected void On(AppClientRenamed @event)
        {
            clients.Rename(@event.Id, @event.Name);
        }

        protected void On(AppClientRevoked @event)
        {
            clients.Revoke(@event.Id);
        }

        protected void On(AppLanguageAdded @event)
        {
            languages.Add(@event.Language);
        }

        protected void On(AppLanguageRemoved @event)
        {
            languages.Remove(@event.Language);
        }

        protected void On(AppMasterLanguageSet @event)
        {
            languages.SetMasterLanguage(@event.Language);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create app");

            ThrowIfCreated();

            var appId = new NamedId<Guid>(command.AggregateId, command.Name);

            RaiseEvent(SimpleMapper.Map(command, new AppCreated { AppId = appId }));

            RaiseEvent(SimpleMapper.Map(command, CreateInitialOwner(appId, command)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialLanguage(appId)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialMasterLanguage(appId)));

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot assign contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command, string secret, DateTime expiresUtc)
        {
            Guard.Valid(command, nameof(command), () => "Cannot attach client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached { Secret = secret, ExpiresUtc = expiresUtc }));

            return this;
        }

        public AppDomainObject RenameClient(RenameClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot rename client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot revoke client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AddLanguage(AddLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot add language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));

            return this;
        }

        public AppDomainObject RemoveLanguage(RemoveLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));

            return this;
        }

        public AppDomainObject SetMasterLanguage(SetMasterLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot set master language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppMasterLanguageSet()));

            return this;
        }

        private static AppLanguageAdded CreateInitialLanguage(NamedId<Guid> id)
        {
            return new AppLanguageAdded { AppId = id, Language = DefaultLanguage };
        }

        private static AppMasterLanguageSet CreateInitialMasterLanguage(NamedId<Guid> id)
        {
            return new AppMasterLanguageSet { AppId = id, Language = DefaultLanguage };
        }

        private static AppContributorAssigned CreateInitialOwner(NamedId<Guid> id, SquidexCommand command)
        {
            return new AppContributorAssigned { AppId = id, ContributorId = command.Actor.Identifier, Permission = PermissionLevel.Owner };
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
