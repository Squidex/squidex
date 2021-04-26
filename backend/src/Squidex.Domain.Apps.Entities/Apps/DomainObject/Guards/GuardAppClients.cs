// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public static class GuardAppClients
    {
        public static void CanAttach(AttachClient command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                var clients = app.Clients;

                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("ClientId"), nameof(command.Id));
                }
                else if (clients.ContainsKey(command.Id))
                {
                    e(T.Get("apps.clients.idAlreadyExists"));
                }
            });
        }

        public static void CanRevoke(RevokeClient command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            GetClientOrThrow(app.Clients, command.Id);

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("ClientId"), nameof(command.Id));
                }
            });
        }

        public static void CanUpdate(UpdateClient command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            GetClientOrThrow(app.Clients, command.Id);

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Id))
                {
                    e(Not.Defined("Clientd"), nameof(command.Id));
                }

                if (command.Role != null && !app.Roles.Contains(command.Role))
                {
                    e(Not.Valid(nameof(command.Role)), nameof(command.Role));
                }

                if (command.ApiCallsLimit != null && command.ApiCallsLimit < 0)
                {
                    e(Not.GreaterEqualsThan(nameof(command.ApiCallsLimit), "0"), nameof(command.ApiCallsLimit));
                }

                if (command.ApiTrafficLimit != null && command.ApiTrafficLimit < 0)
                {
                    e(Not.GreaterEqualsThan(nameof(command.ApiTrafficLimit), "0"), nameof(command.ApiTrafficLimit));
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
