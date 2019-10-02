// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppContributors : ArrayDictionary<string, string>
    {
        public static readonly AppContributors Empty = new AppContributors();

        private AppContributors()
        {
        }

        public AppContributors(KeyValuePair<string, string>[] items)
            : base(items)
        {
        }

        [Pure]
        public AppContributors Assign(string contributorId, string role)
        {
            Guard.NotNullOrEmpty(contributorId);
            Guard.NotNullOrEmpty(role);

            return new AppContributors(With(contributorId, role));
        }

        [Pure]
        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId);

            return new AppContributors(Without(contributorId));
        }
    }
}
