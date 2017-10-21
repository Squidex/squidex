// ==========================================================================
//  AppContributors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public class AppContributors
    {
        private readonly Dictionary<string, AppContributorPermission> contributors = new Dictionary<string, AppContributorPermission>();

        public IReadOnlyDictionary<string, AppContributorPermission> Contributors
        {
            get { return contributors; }
        }

        public void Assign(string contributorId, AppContributorPermission permission)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.Enum(permission, nameof(permission));

            contributors[contributorId] = permission;
        }

        public void Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            contributors.Remove(contributorId);
        }
    }
}
