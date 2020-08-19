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

        public bool AllowAnonymous { get; set; }

        public long ApiCallsLimit { get; }

        public AppClient(string name, string secret, string role, bool allowAnonymous = false, long apiCallsLimit = 0)
            : base(name)
        {
            Guard.NotNullOrEmpty(secret, nameof(secret));
            Guard.NotNullOrEmpty(role, nameof(role));
            Guard.GreaterEquals(apiCallsLimit, 0, nameof(apiCallsLimit));

            Role = role;

            Secret = secret;

            AllowAnonymous = allowAnonymous;

            ApiCallsLimit = apiCallsLimit;
        }

        [Pure]
        public AppClient Update(string? name, string? role, bool? allowAnonymous, long? apiCallsLimit)
        {
            return new AppClient(name.Or(Name), Secret, role.Or(Role), allowAnonymous ?? AllowAnonymous, apiCallsLimit ?? ApiCallsLimit);
        }
    }
}
