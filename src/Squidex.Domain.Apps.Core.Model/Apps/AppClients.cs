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
    public sealed class AppClients : ArrayDictionary<string, AppClient>
    {
        public static readonly AppClients Empty = new AppClients();

        private AppClients()
        {
        }

        public AppClients(KeyValuePair<string, AppClient>[] items)
            : base(items)
        {
        }

        [Pure]
        public AppClients Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return new AppClients(Without(id));
        }

        [Pure]
        public AppClients Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(client, nameof(client));

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            return new AppClients(With(id, client));
        }

        [Pure]
        public AppClients Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (ContainsKey(id))
            {
                throw new ArgumentException("Id already exists.", nameof(id));
            }

            return new AppClients(With(id, new AppClient(id, secret, Role.Editor)));
        }

        [Pure]
        public AppClients Rename(string id, string newName)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return new AppClients(With(id, client.Rename(newName)));
        }

        [Pure]
        public AppClients Update(string id, string role)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return new AppClients(With(id, client.Update(role)));
        }
    }
}
