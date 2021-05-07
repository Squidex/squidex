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

            if (!this.TrySet(contributorId, role, out var updated))
            {
                return this;
            }

            return new AppContributors(updated);
        }

        [Pure]
        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            if (!this.TryRemove(contributorId, out var updated))
            {
                return this;
            }

            return new AppContributors(updated);
        }
    }
}
