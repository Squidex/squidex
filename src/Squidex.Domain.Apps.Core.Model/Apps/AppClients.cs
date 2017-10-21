// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public class AppClients
    {
        private readonly Dictionary<string, AppClient> clients = new Dictionary<string, AppClient>();

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients; }
        }

        public void Add(string id, string secret)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            clients.Add(id, new AppClient(secret, id));
        }

        public void Revoke(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            clients.Remove(id);
        }
    }
}
