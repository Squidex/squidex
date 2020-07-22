﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppContributors
    {
        public static Task CanAssign(AppContributors contributors, Roles roles, AssignContributor command, IUserResolver users, IAppLimitsPlan? plan)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(async e =>
            {
                if (!roles.Contains(command.Role))
                {
                    e(Not.Valid("role"), nameof(command.Role));
                }

                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    e(Not.Defined("Contributor id"), nameof(command.ContributorId));
                }
                else
                {
                    var user = await users.FindByIdAsync(command.ContributorId);

                    if (user == null)
                    {
                        throw new DomainObjectNotFoundException(command.ContributorId);
                    }

                    if (!command.Restoring)
                    {
                        if (string.Equals(command.ContributorId, command.Actor?.Identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new DomainForbiddenException("You cannot change your own role.");
                        }

                        if (!contributors.TryGetValue(command.ContributorId, out var role))
                        {
                            if (plan != null && plan.MaxContributors > 0 && contributors.Count >= plan.MaxContributors)
                            {
                                e("You have reached the maximum number of contributors for your plan.");
                            }
                        }
                    }
                }
            });
        }

        public static void CanRemove(AppContributors contributors, RemoveContributor command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(e =>
            {
                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    e(Not.Defined("Contributor id"), nameof(command.ContributorId));
                }

                var ownerIds = contributors.Where(x => x.Value == Role.Owner).Select(x => x.Key).ToList();

                if (ownerIds.Count == 1 && ownerIds.Contains(command.ContributorId))
                {
                    e("Cannot remove the only owner.");
                }
            });

            if (!contributors.ContainsKey(command.ContributorId))
            {
                throw new DomainObjectNotFoundException(command.ContributorId);
            }
        }
    }
}
