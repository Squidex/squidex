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
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppContributors
    {
        private readonly Dictionary<string, AppContributorPermission> contributors = new Dictionary<string, AppContributorPermission>();

        public int Count
        {
            get { return contributors.Count; }
        }

        public void Assign(string contributorId, AppContributorPermission permission)
        {
            string Message() => "Cannot assign contributor";

            ThrowIfFound(contributorId, permission, Message);
            ThrowIfNoOwner(c => c[contributorId] = permission, Message);

            contributors[contributorId] = permission;
        }

        public void Remove(string contributorId)
        {
            string Message() => "Cannot remove contributor";

            ThrowIfNotFound(contributorId);
            ThrowIfNoOwner(c => c.Remove(contributorId), Message);

            contributors.Remove(contributorId);
        }

        private void ThrowIfNotFound(string contributorId)
        {
            if (!contributors.ContainsKey(contributorId))
            {
                throw new DomainObjectNotFoundException(contributorId, "Contributors", typeof(AppDomainObject));
            }
        }

        private void ThrowIfFound(string contributorId, AppContributorPermission permission, Func<string> message)
        {
            if (contributors.TryGetValue(contributorId, out var currentPermission) && currentPermission == permission)
            {
                var error = new ValidationError("Contributor is already part of the app with same permissions.", "ContributorId");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfNoOwner(Action<Dictionary<string, AppContributorPermission>> change, Func<string> message)
        {
            var contributorsCopy = new Dictionary<string, AppContributorPermission>(contributors);

            change(contributorsCopy);

            if (contributorsCopy.All(x => x.Value != AppContributorPermission.Owner))
            {
                var error = new ValidationError("Contributor is the last owner.", "ContributorId");

                throw new ValidationException(message(), error);
            }
        }
    }
}
