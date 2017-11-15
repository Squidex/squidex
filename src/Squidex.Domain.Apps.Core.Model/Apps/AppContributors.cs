// ==========================================================================
//  AppContributors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppContributors : DictionaryWrapper<string, AppContributorPermission>
    {
        public static readonly AppContributors Empty = new AppContributors();

        private AppContributors()
            : base(ImmutableDictionary<string, AppContributorPermission>.Empty)
        {
        }

        public AppContributors(ImmutableDictionary<string, AppContributorPermission> inner)
            : base(inner)
        {
        }

        public AppContributors Assign(string contributorId, AppContributorPermission permission)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.Enum(permission, nameof(permission));

            return new AppContributors(Inner.SetItem(contributorId, permission));
        }

        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            return new AppContributors(Inner.Remove(contributorId));
        }
    }
}
