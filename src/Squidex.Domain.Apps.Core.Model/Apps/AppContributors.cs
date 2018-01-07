// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using System.Diagnostics.Contracts;
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

        [Pure]
        public AppContributors Assign(string contributorId, AppContributorPermission permission)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.Enum(permission, nameof(permission));

            return new AppContributors(Inner.SetItem(contributorId, permission));
        }

        [Pure]
        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            return new AppContributors(Inner.Remove(contributorId));
        }
    }
}
