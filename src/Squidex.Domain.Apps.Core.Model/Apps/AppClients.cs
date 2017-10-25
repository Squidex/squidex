// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class AppClients : DictionaryBase<string, AppClient>
    {
        public void Add(string id, AppClient client)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(client, nameof(client));

            Inner.Add(id, client);
        }

        public void Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            Inner.Add(id, new AppClient(id, secret, AppClientPermission.Editor));
        }

        public void Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            Inner.Remove(id);
        }
    }
}
