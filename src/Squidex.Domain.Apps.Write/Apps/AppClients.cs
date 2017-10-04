// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps
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
            ThrowIfFound(id, () => "Cannot add client");

            clients[id] = new AppClient(secret, id, AppClientPermission.Editor);
        }

        public void Rename(string clientId, string name)
        {
            ThrowIfNotFound(clientId);

            clients[clientId] = clients[clientId].Rename(name, () => "Cannot rename client");
        }

        public void Update(string clientId, AppClientPermission permission)
        {
            ThrowIfNotFound(clientId);

            clients[clientId] = clients[clientId].Update(permission, () => "Cannot update client");
        }

        public void Revoke(string clientId)
        {
            ThrowIfNotFound(clientId);

            clients.Remove(clientId);
        }

        private void ThrowIfNotFound(string clientId)
        {
            if (!clients.ContainsKey(clientId))
            {
                throw new DomainObjectNotFoundException(clientId, "Contributors", typeof(AppDomainObject));
            }
        }

        private void ThrowIfFound(string clientId, Func<string> message)
        {
            if (clients.ContainsKey(clientId))
            {
                var error = new ValidationError("Client id is alreay part of the app", "Id");

                throw new ValidationException(message(), error);
            }
        }
    }
}
