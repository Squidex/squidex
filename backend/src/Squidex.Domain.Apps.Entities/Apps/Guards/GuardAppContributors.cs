// ==========================================================================
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
using Squidex.Infrastructure.Translations;
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
                    e(Not.Valid(nameof(command.Role)), nameof(command.Role));
                }

                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    e(Not.Defined(nameof(command.ContributorId)), nameof(command.ContributorId));
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
                            throw new DomainForbiddenException(T.Get("apps.contributors.cannotChangeYourself"));
                        }

                        if (!contributors.TryGetValue(command.ContributorId, out var role))
                        {
                            if (plan != null && plan.MaxContributors > 0 && contributors.Count >= plan.MaxContributors)
                            {
                                e(T.Get("apps.contributors.maxReached"));
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
                    e(Not.Defined(nameof(command.ContributorId)), nameof(command.ContributorId));
                }

                var ownerIds = contributors.Where(x => x.Value == Role.Owner).Select(x => x.Key).ToList();

                if (ownerIds.Count == 1 && ownerIds.Contains(command.ContributorId))
                {
                    e(T.Get("apps.contributors.onlyOneOwner"));
                }
            });

            if (!contributors.ContainsKey(command.ContributorId))
            {
                throw new DomainObjectNotFoundException(command.ContributorId);
            }
        }
    }
}
