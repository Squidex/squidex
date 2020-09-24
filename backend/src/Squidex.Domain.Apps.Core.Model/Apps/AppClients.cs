// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

        public AppClients(Dictionary<string, AppClient> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppClients Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return Without<AppClients>(id);
        }

        [Pure]
        public AppClients Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(client, nameof(client));

            return With<AppClients>(id, client);
        }

        [Pure]
        public AppClients Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            var newClient = new AppClient(id, secret, Role.Editor);

            return Add(id, newClient);
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

            var newClient = client.Update(name, role, apiCallsLimit, apiTrafficLimit, allowAnonymous);

            return With<AppClients>(id, newClient);
        }
    }
}
