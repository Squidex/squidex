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
    public sealed class AppContributors : DictionaryWrapper<string, string>
    {
        public static readonly AppContributors Empty = new AppContributors();

        private AppContributors()
            : base(ImmutableDictionary<string, string>.Empty)
        {
        }

        public AppContributors(ImmutableDictionary<string, string> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppContributors Assign(string contributorId, string role)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));
            Guard.NotNullOrEmpty(role, nameof(role));

            return new AppContributors(Inner.SetItem(contributorId, role));
        }

        [Pure]
        public AppContributors Remove(string contributorId)
        {
            Guard.NotNullOrEmpty(contributorId, nameof(contributorId));

            return new AppContributors(Inner.Remove(contributorId));
        }
    }
}
