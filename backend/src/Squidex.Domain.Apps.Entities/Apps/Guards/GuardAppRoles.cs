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
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppRoles
    {
        public static void CanAdd(Roles roles, AddRole command)
        {
            Guard.NotNull(command);

            Validate.It(() => "Cannot add role.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                   e(Not.Defined("Name"), nameof(command.Name));
                }
                else if (roles.Contains(command.Name))
                {
                    e("A role with the same name already exists.");
                }
            });
        }

        public static void CanDelete(Roles roles, DeleteRole command, AppContributors contributors, AppClients clients)
        {
            Guard.NotNull(command);

            CheckRoleExists(roles, command.Name);

            Validate.It(() => "Cannot delete role.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                   e(Not.Defined("Name"), nameof(command.Name));
                }
                else if (Roles.IsDefault(command.Name))
                {
                    e("Cannot delete a default role.");
                }

                if (clients.Values.Any(x => string.Equals(x.Role, command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e("Cannot remove a role when a client is assigned.");
                }

                if (contributors.Values.Any(x => string.Equals(x, command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    e("Cannot remove a role when a contributor is assigned.");
                }
            });
        }

        public static void CanUpdate(Roles roles, UpdateRole command)
        {
            Guard.NotNull(command);

            CheckRoleExists(roles, command.Name);

            Validate.It(() => "Cannot delete role.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                   e(Not.Defined("Name"), nameof(command.Name));
                }
                else if (Roles.IsDefault(command.Name))
                {
                    e("Cannot update a default role.");
                }

                if (command.Permissions == null)
                {
                   e(Not.Defined("Permissions"), nameof(command.Permissions));
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
                throw new DomainObjectNotFoundException(name, "Roles", typeof(IAppEntity));
            }
        }
    }
}
