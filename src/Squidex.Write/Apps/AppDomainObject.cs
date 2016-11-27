// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Core.Apps;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Write.Apps.Commands;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public sealed class AppDomainObject : DomainObject
    {
        private static readonly List<Language> DefaultLanguages = new List<Language> { Language.GetLanguage("en") };
        private readonly Dictionary<string, AppClient> clients = new Dictionary<string, AppClient>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PermissionLevel> contributors = new Dictionary<string, PermissionLevel>();
        private string name;

        public string Name
        {
            get { return name; }
        }

        public IReadOnlyDictionary<string, PermissionLevel> Contributors
        {
            get { return contributors; }
        }

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients; }
        }

        public AppDomainObject(Guid id, int version) 
            : base(id, version)
        {
        }

        public void On(AppCreated @event)
        {
            name = @event.Name;
        }

        public void On(AppContributorAssigned @event)
        {
            contributors[@event.ContributorId] = @event.Permission;
        }

        public void On(AppContributorRemoved @event)
        {
            contributors.Remove(@event.ContributorId);
        }

        public void On(AppClientAttached @event)
        {
            clients.Add(@event.ClientName, new AppClient(@event.ClientName, @event.ClientSecret, @event.ExpiresUtc));
        }

        public void On(AppClientRevoked @event)
        {
            clients.Remove(@event.ClientName);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            Func<string> message = () => "Cannot create app";

            Guard.Valid(command, nameof(command), message);

            ThrowIfCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppCreated()));

            RaiseEvent(CreateInitialOwner(command));
            RaiseEvent(CreateInitialLanguage());

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Func<string> message = () => "Cannot assign contributor";

            Guard.Valid(command, nameof(command), message);

            ThrowIfNotCreated();
            ThrowIfNoOwner(c => c[command.ContributorId] = command.Permission, message);

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            Func<string> message = () => "Cannot revoke client";

            Guard.Valid(command, nameof(command), () => "Cannot revoke client");

            ThrowIfNotCreated();
            ThrowIfClientNotFound(command, message);

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command, string secret)
        {
            Func<string> message = () => "Cannot attach client";

            Guard.Valid(command, nameof(command), () => "Cannot attach client");

            ThrowIfNotCreated();
            ThrowIfClientFound(command, message);

            var expire = command.Timestamp.AddYears(1);

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached { ClientSecret = secret, ExpiresUtc = expire }));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Func<string> message = () => "Cannot remove contributor";

            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            ThrowIfNotCreated();
            ThrowIfContributorNotFound(command, message);

            ThrowIfNoOwner(c => c.Remove(command.ContributorId), message);

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject ConfigureLanguages(ConfigureLanguages command)
        {
            Func<string> message = () => "Cannot configure languages";

            Guard.Valid(command, nameof(command), message);

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguagesConfigured()));

            return this;
        }

        private static AppLanguagesConfigured CreateInitialLanguage()
        {
            return new AppLanguagesConfigured { Languages = DefaultLanguages };
        }

        private static AppContributorAssigned CreateInitialOwner(IUserCommand command)
        {
            return new AppContributorAssigned { ContributorId = command.UserId, Permission = PermissionLevel.Owner };
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

        private void ThrowIfClientFound(AttachClient command, Func<string> message)
        {
            if (clients.ContainsKey(command.ClientName))
            {
                var error = new ValidationError("Client name is alreay part of the app", "ClientName");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfClientNotFound(RevokeClient command, Func<string> message)
        {
            if (!clients.ContainsKey(command.ClientName))
            {
                var error = new ValidationError("Client is not part of the app", "ClientName");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfContributorNotFound(RemoveContributor command, Func<string> message)
        {
            if (!contributors.ContainsKey(command.ContributorId))
            {
                var error = new ValidationError("Contributor is not part of the app", "ContributorId");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfNoOwner(Action<Dictionary<string, PermissionLevel>> change, Func<string> message)
        {
            var contributorsCopy = new Dictionary<string, PermissionLevel>(contributors);

            change(contributorsCopy);

            if (contributorsCopy.All(x => x.Value != PermissionLevel.Owner))
            {
                var error = new ValidationError("Contributor is the last owner", "ContributorId");

                throw new ValidationException(message(), error);
            }
        }
    }
}
