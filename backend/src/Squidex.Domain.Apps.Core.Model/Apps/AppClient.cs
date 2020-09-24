// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AppClient : Named
    {
        public string Role { get; }

        public string Secret { get; }

        public long ApiCallsLimit { get; }

        public long ApiTrafficLimit { get; }

        public bool AllowAnonymous { get; }

        public AppClient(string name, string secret, string role, long apiCallsLimit = 0, long apiTrafficLimit = 0, bool allowAnonymous = false)
            : base(name)
        {
            Guard.NotNullOrEmpty(secret, nameof(secret));
            Guard.NotNullOrEmpty(role, nameof(role));
            Guard.GreaterEquals(apiCallsLimit, 0, nameof(apiCallsLimit));
            Guard.GreaterEquals(apiTrafficLimit, 0, nameof(apiTrafficLimit));

            Secret = secret;

            Role = role;

            ApiCallsLimit = apiCallsLimit;
            ApiTrafficLimit = apiTrafficLimit;

            AllowAnonymous = allowAnonymous;
        }

        [Pure]
        public AppClient Update(string? name, string? role,
            long? apiCallsLimit,
            long? apiTrafficLimit,
            bool? allowAnonymous)
        {
            return new AppClient(name.Or(Name), Secret, role.Or(Role),
                apiCallsLimit ?? ApiCallsLimit,
                apiTrafficLimit ?? ApiTrafficLimit,
                allowAnonymous ?? AllowAnonymous);
        }
    }
}
