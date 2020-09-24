// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppRoles
    {
        public static void CanAdd(Roles roles, AddRole command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }
                else if (roles.Contains(command.Name))
                {
                    e(T.Get("apps.roles.nameAlreadyExists"));
                }
            });
        }

        public static void CanDelete(Roles roles, DeleteRole command, AppContributors contributors, AppClients clients)
        {
            Guard.NotNull(command, nameof(command));

            CheckRoleExists(roles, command.Name);

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }
                else if (Roles.IsDefault(command.Name))
                {
                    e(T.Get("apps.roles.defaultRoleNotRemovable"));
                }

                if (clients.Values.Any(x => string.Equals(x.Role, command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e(T.Get("apps.roles.usedRoleByClientsNotRemovable"));
                }

                if (contributors.Values.Any(x => string.Equals(x, command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e(T.Get("apps.roles.usedRoleByContributorsNotRemovable"));
                }
            });
        }

        public static void CanUpdate(Roles roles, UpdateRole command)
        {
            Guard.NotNull(command, nameof(command));

            CheckRoleExists(roles, command.Name);

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined(nameof(command.Name)), nameof(command.Name));
                }
                else if (Roles.IsDefault(command.Name))
                {
                    e(T.Get("apps.roles.defaultRoleNotUpdateable"));
                }

                if (command.Permissions == null)
                {
                    e(Not.Defined(nameof(command.Permissions)), nameof(command.Permissions));
                }
            });
        }

        private static void CheckRoleExists(Roles roles, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || Roles.IsDefault(name))
            {
                return;
            }

            if (!roles.ContainsCustom(name))
            {
                throw new DomainObjectNotFoundException(name);
            }
        }
    }
}
