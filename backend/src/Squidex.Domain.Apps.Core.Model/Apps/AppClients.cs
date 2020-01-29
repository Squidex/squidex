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
using Squidex.Infrastructure.Reflection.Equality;

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
            Guard.NotNullOrEmpty(id);

            return Without<AppClients>(id);
        }

        [Pure]
        public AppClients Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id);
            Guard.NotNull(client);

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            return With<AppClients>(id, client);
        }

        [Pure]
        public AppClients Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id);

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            return With<AppClients>(id, new AppClient(id, secret, Role.Editor));
        }

        [Pure]
        public AppClients Rename(string id, string newName)
        {
            Guard.NotNullOrEmpty(id);

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return With<AppClients>(id, client.Rename(newName), DeepEqualityComparer<AppClient>.Default);
        }

        [Pure]
        public AppClients Update(string id, string role)
        {
            Guard.NotNullOrEmpty(id);

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return With<AppClients>(id, client.Update(role), DeepEqualityComparer<AppClient>.Default);
        }
    }
}
