// ==========================================================================
//  AppContributors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppContributors : DictionaryBase<string, AppContributorPermission>
    {
        public void Assign(string contributorId, AppContributorPermission permission)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.Enum(permission, nameof(permission));

            Inner[contributorId] = permission;
        }

        public void Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            Inner.Remove(contributorId);
        }
    }
}
