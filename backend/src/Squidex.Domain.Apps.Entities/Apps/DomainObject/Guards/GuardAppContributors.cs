// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public static class GuardAppContributors
    {
        public static Task CanAssign(AssignContributor command, IAppEntity app, IUserResolver users, IAppLimitsPlan plan)
        {
            Guard.NotNull(command, nameof(command));

            var contributors = app.Contributors;

            return Validate.It(async e =>
            {
                if (!app.Roles.Contains(command.Role))
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

                        if (!contributors.TryGetValue(command.ContributorId, out _))
                        {
                            if (plan.MaxContributors > 0 && contributors.Count >= plan.MaxContributors)
                            {
                                e(T.Get("apps.contributors.maxReached"));
                            }
                        }
                    }
                }
            });
        }

        public static void CanRemove(RemoveContributor command, IAppEntity app)
        {
            Guard.NotNull(command, nameof(command));

            var contributors = app.Contributors;

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
