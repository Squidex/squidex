// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppClients
    {
        public static void CanAttach(AppClients clients, AttachClient command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("ClientId"), nameof(command.Id));
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

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("ClientId"), nameof(command.Id));
                }
            });
        }

        public static void CanUpdate(AppClients clients, UpdateClient command, Roles roles)
        {
            Guard.NotNull(command, nameof(command));

            GetClientOrThrow(clients, command.Id);

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("Clientd"), nameof(command.Id));
                }

                if (command.Role != null && !roles.Contains(command.Role))
                {
                    e(Not.Valid("Role"), nameof(command.Role));
                }
            });
        }

        private static AppClient? GetClientOrThrow(AppClients clients, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (!clients.TryGetValue(id, out var client))
            {
                throw new DomainObjectNotFoundException(id);
            }

            return client;
        }
    }
}
