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
    public sealed class AppClients : ImmutableDictionary<string, AppClient>
    {
        public static readonly AppClients Empty = new AppClients();

        private AppClients()
        {
        }

        public AppClients(IDictionary<string, AppClient> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppClients Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!this.TryRemove(id, out var updated))
            {
                return this;
            }

            return new AppClients(updated);
        }

        [Pure]
        public AppClients Add(string id, string secret, string? role = null)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNullOrEmpty(secret, nameof(secret));

            var newClient = new AppClient(id, secret)
            {
                Role = role.Or(Role.Editor)
            };

            if (!this.TryAdd(id, newClient, out var updated))
            {
                return this;
            }

            return new AppClients(updated);
        }

        [Pure]
        public AppClients Update(string id, string? name = null, string? role = null,
            long? apiCallsLimit = null,
            long? apiTrafficLimit = null,
            bool? allowAnonymous = false)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            var newClient = client with
            {
                AllowAnonymous = allowAnonymous ?? client.AllowAnonymous
            };

            if (!string.IsNullOrWhiteSpace(name))
            {
                newClient = newClient with { Name = name };
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                newClient = newClient with { Role = role };
            }

            if (apiCallsLimit >= 0)
            {
                newClient = newClient with { ApiCallsLimit = apiCallsLimit.Value };
            }

            if (apiTrafficLimit >= 0)
            {
                newClient = newClient with { ApiTrafficLimit = apiTrafficLimit.Value };
            }

            if (!this.TrySet(id, newClient, out var updated))
            {
                return this;
            }

            return new AppClients(updated);
        }
    }
}
