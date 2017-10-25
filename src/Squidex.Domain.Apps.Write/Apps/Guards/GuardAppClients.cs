// ==========================================================================
//  GuardAppClients.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public static class GuardAppClients
    {
        public static void CanAttach(AppClients clients, AttachClient command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot attach client.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    error(new ValidationError("Client id must be defined.", nameof(command.Id)));
                }
                else if (clients.ContainsKey(command.Id))
                {
                    error(new ValidationError("Client id already added.", nameof(command.Id)));
                }
            });
        }

        public static void CanRevoke(AppClients clients, RevokeClient command)
        {
            Guard.NotNull(command, nameof(command));

            GetClientOrThrow(clients, command.Id);

            Validate.It(() => "Cannot revoke client.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    error(new ValidationError("Client id must be defined.", nameof(command.Id)));
                }
            });
        }

        public static void CanUpdate(AppClients clients, UpdateClient command)
        {
            Guard.NotNull(command, nameof(command));

            var client = GetClientOrThrow(clients, command.Id);

            Validate.It(() => "Cannot revoke client.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    error(new ValidationError("Client id must be defined.", nameof(command.Id)));
                }

                if (string.IsNullOrWhiteSpace(command.Name) && command.Permission == null)
                {
                    error(new ValidationError("Either name or permission must be defined.", nameof(command.Name), nameof(command.Permission)));
                }

                if (command.Permission.HasValue && !command.Permission.Value.IsEnumValue())
                {
                    error(new ValidationError("Permission is not valid.", nameof(command.Permission)));
                }

                if (client != null)
                {
                    if (!string.IsNullOrWhiteSpace(command.Name) && string.Equals(client.Name, command.Name))
                    {
                        error(new ValidationError("Client already has this name.", nameof(command.Permission)));
                    }

                    if (command.Permission == client.Permission)
                    {
                        error(new ValidationError("Client already has this permission.", nameof(command.Permission)));
                    }
                }
            });
        }

        private static AppClient GetClientOrThrow(AppClients clients, string id)
        {
            if (id == null)
            {
                return null;
            }

            if (!clients.TryGetValue(id, out var client))
            {
                throw new DomainObjectNotFoundException(id, "Clients", typeof(AppDomainObject));
            }

            return client;
        }
    }
}
