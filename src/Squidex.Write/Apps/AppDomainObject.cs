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
        private readonly HashSet<string> clientKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

        public void On(AppClientKeyCreated @event)
        {
            clientKeys.Add(@event.ClientKey);
        }

        public void On(AppClientKeyRevoked @event)
        {
            clientKeys.Remove(@event.ClientKey);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create app");

            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppCreated()));

            RaiseEvent(CreateInitialOwner(command));
            RaiseEvent(CreateInitialLanguage());

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot assign contributor");

            VerifyCreated();
            VerifyOwnership(c => c[command.ContributorId] = command.Permission);

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            VerifyCreated();
            VerifyContributorFound(command);
            VerifyOwnership(c => c.Remove(command.ContributorId));

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject ConfigureLanguages(ConfigureLanguages command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            VerifyCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguagesConfigured()));

            return this;
        }

        public AppDomainObject RevokeClientKey(RevokeClientKey command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot revoke client key");

            VerifyCreated();
            VerifyClientKeyFound(command);

            RaiseEvent(SimpleMapper.Map(command, new AppClientKeyRevoked()));

            return this;
        }

        public AppDomainObject CreateClientKey(CreateClientKey command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create client key");

            VerifyCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientKeyCreated()));

            return this;
        }

        private static AppLanguagesConfigured CreateInitialLanguage()
        {
            return new AppLanguagesConfigured { Languages = DefaultLanguages };
        }

        private static AppContributorAssigned CreateInitialOwner(ISubjectCommand command)
        {
            return new AppContributorAssigned { ContributorId = command.SubjectId, Permission = PermissionLevel.Owner };
        }

        private void VerifyCreated()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has not been created.");
            }
        }

        private void VerifyNotCreated()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has already been created.");
            }
        }

        private void VerifyClientKeyFound(RevokeClientKey command)
        {
            if (!clientKeys.Contains(command.ClientKey))
            {
                var error = new ValidationError("Client key is not part of the app", "ClientKey");

                throw new ValidationException("Cannot revoke client key", error);
            }
        }

        private void VerifyContributorFound(RemoveContributor command)
        {
            if (!contributors.ContainsKey(command.ContributorId))
            {
                var error = new ValidationError("Contributor is not part of the app", "ContributorId");

                throw new ValidationException("Cannot remove contributor", error);
            }
        }

        private void VerifyOwnership(Action<Dictionary<string, PermissionLevel>> change)
        {
            var contributorsCopy = new Dictionary<string, PermissionLevel>(contributors);

            change(contributorsCopy);

            if (contributorsCopy.All(x => x.Value != PermissionLevel.Owner))
            {
                var error = new ValidationError("Contributor is the last owner", "ContributorId");

                throw new ValidationException("Cannot assign contributor", error);
            }
        }
    }
}
