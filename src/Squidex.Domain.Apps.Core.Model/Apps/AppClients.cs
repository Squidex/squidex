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
    public sealed class AppClients : DictionaryWrapper<string, AppClient>
    {
        public static readonly AppClients Empty = new AppClients();

        private AppClients()
            : base(ImmutableDictionary<string, AppClient>.Empty)
        {
        }

        public AppClients(ImmutableDictionary<string, AppClient> inner)
            : base(inner)
        {
        }

        [Pure]
        public AppClients Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(client, nameof(client));

            return new AppClients(Inner.Add(id, client));
        }

        [Pure]
        public AppClients Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return new AppClients(Inner.Add(id, new AppClient(id, secret, AppClientPermission.Editor)));
        }

        [Pure]
        public AppClients Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return new AppClients(Inner.Remove(id));
        }

        [Pure]
        public AppClients Rename(string id, string newName)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return new AppClients(Inner.SetItem(id, client.Rename(newName)));
        }

        [Pure]
        public AppClients Update(string id, AppClientPermission permission)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return new AppClients(Inner.SetItem(id, client.Update(permission)));
        }
    }
}
