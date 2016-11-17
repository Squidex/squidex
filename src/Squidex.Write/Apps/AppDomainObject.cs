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
using Squidex.Infrastructure.Reflection;
// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public sealed class AppDomainObject : DomainObject
    {
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

        public AppDomainObject(Guid id, int version) : base(id, version)
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

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create app");

            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppCreated()));
            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned { ContributorId = command.SubjectId, Permission = PermissionLevel.Owner }));

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot assign contributor");

            VerifyCreated();
            VerifyHasStillOwner(c => c[command.ContributorId] = command.Permission);

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            VerifyCreated();
            VerifyContributorFound(command);
            VerifyHasStillOwner(c => c.Remove(command.ContributorId));

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
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

        private void VerifyContributorFound(RemoveContributor command)
        {
            if (!contributors.ContainsKey(command.ContributorId))
            {
                var error = new ValidationError("Contributor is not part of the app", "ContributorId");

                throw new ValidationException("Cannot remove contributor", error);
            }
        }

        private void VerifyHasStillOwner(Action<Dictionary<string, PermissionLevel>> change)
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
