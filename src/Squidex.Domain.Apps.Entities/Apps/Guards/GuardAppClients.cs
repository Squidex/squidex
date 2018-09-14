// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppClients
    {
        public static void CanAttach(AppClients clients, AttachClient command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot attach client.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e("Client id is required.", nameof(command.Id));
                }
                else if (clients.ContainsKey(command.Id))
                {
                    e("A client with the same id already exists.");
                }
            });
        }

        public static void CanRevoke(AppClients clients, RevokeClient command)
        {
            Guard.NotNull(command, nameof(command));

            GetClientOrThrow(clients, command.Id);

            Validate.It(() => "Cannot revoke client.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e("Client id is required.", nameof(command.Id));
                }
            });
        }

        public static void CanUpdate(AppClients clients, UpdateClient command)
        {
            Guard.NotNull(command, nameof(command));

            var client = GetClientOrThrow(clients, command.Id);

            Validate.It(() => "Cannot update client.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e("Client id is required.", nameof(command.Id));
                }

                if (string.IsNullOrWhiteSpace(command.Name) && command.Permission == null)
                {
                    e("Either name or permission must be defined.", nameof(command.Name), nameof(command.Permission));
                }

                if (command.Permission.HasValue && !command.Permission.Value.IsEnumValue())
                {
                    e("Permission is not valid.", nameof(command.Permission));
                }

                if (client == null)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(command.Name) && string.Equals(client.Name, command.Name))
                {
                    e("Client has already this name.", nameof(command.Name));
                }

                if (command.Permission == client.Permission)
                {
                    e("Client has already this permission.", nameof(command.Permission));
                }
            });
        }

        private static AppClient GetClientOrThrow(AppClients clients, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (!clients.TryGetValue(id, out var client))
            {
                throw new DomainObjectNotFoundException(id, "Clients", typeof(IAppEntity));
            }

            return client;
        }
    }
}
