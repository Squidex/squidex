// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppContributors : ImmutableDictionary<string, string>
    {
        public static readonly AppContributors Empty = new AppContributors();

        private AppContributors()
        {
        }

        public AppContributors(IDictionary<string, string> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppContributors Assign(string contributorId, string role)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.NotNullOrEmpty(role, nameof(role));

            return Set<AppContributors>(contributorId, role, EqualityComparer<string>.Default);
        }

        [Pure]
        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            return RemoveKey<AppContributors>(contributorId);
        }
    }
}
