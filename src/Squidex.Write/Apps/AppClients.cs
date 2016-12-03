// ==========================================================================
//  AppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
// ReSharper disable InvertIf

namespace Squidex.Write.Apps
{
    public class AppClients
    {
        private readonly Dictionary<string, AppClient> clients = new Dictionary<string, AppClient>();

        public IReadOnlyDictionary<string, AppClient> Clients
        {
            get { return clients; }
        }

        public void Add(string id, string secret, DateTime expires)
        {
            Func<string> message = () => "Cannot add client";

            ThrowIfFound(id, message);

            clients[id] = new AppClient(id, secret, expires);
        }

        public void Rename(string clientId, string name)
        {
            Func<string> message = () => "Cannot rename client";

            ThrowIfNotFound(clientId);
            ThrowIfSameName(clientId, name, message);

            clients[clientId] = clients[clientId].Rename(name);
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

        private void ThrowIfSameName(string clientId, string name, Func<string> message)
        {
            if (string.Equals(clients[clientId].Name, name))
            {
                var error = new ValidationError("Client already has the name", "Id");

                throw new ValidationException(message(), error);
            }
        }
    }
}
