// ==========================================================================
//  AppContributors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Core.Apps;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public class AppContributors
    {
        private readonly Dictionary<string, PermissionLevel> contributors = new Dictionary<string, PermissionLevel>();

        public void Assign(string contributorId, PermissionLevel permission)
        {
            Func<string> message = () => "Cannot assign contributor";

            ThrowIfFound(contributorId, permission, message);
            ThrowIfNoOwner(c => c[contributorId] = permission, message);

            contributors[contributorId] = permission;
        }

        public void Remove(string contributorId)
        {
            Func<string> message = () => "Cannot remove contributor";

            ThrowIfNotFound(contributorId);
            ThrowIfNoOwner(c => c.Remove(contributorId), message);

            contributors.Remove(contributorId);
        }

        private void ThrowIfNotFound(string contributorId)
        {
            if (!contributors.ContainsKey(contributorId))
            {
                throw new DomainObjectNotFoundException(contributorId, "Contributors", typeof(AppDomainObject));
            }
        }

        private void ThrowIfFound(string contributorId, PermissionLevel permission, Func<string> message)
        {
            PermissionLevel currentPermission;

            if (contributors.TryGetValue(contributorId, out currentPermission) && currentPermission == permission)
            {
                var error = new ValidationError("Contributor is already part of the app with same permissions", "ContributorId");

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
