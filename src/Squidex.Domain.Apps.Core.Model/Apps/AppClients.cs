// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
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

        public AppClients Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(client, nameof(client));

            return new AppClients(Inner.Add(id, client));
        }

        public AppClients Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return new AppClients(Inner.Add(id, new AppClient(id, secret, AppClientPermission.Editor)));
        }

        public AppClients Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return new AppClients(Inner.Remove(id));
        }

        public AppClients Rename(string id, string newName)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!TryGetValue(id, out var client))
            {
                return this;
            }

            return new AppClients(Inner.SetItem(id, client.Rename(newName)));
        }

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
