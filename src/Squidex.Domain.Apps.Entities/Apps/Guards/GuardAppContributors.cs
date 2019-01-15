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
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppContributors
    {
        public static Task CanAssign(AppContributors contributors, AssignContributor command, IUserResolver users, IAppLimitsPlan plan, Roles roles)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot assign contributor.", async e =>
            {
                if (!roles.ContainsKey(command.Role))
                {
                    e(Not.Valid("role"), nameof(command.Role));
                }

                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    e(Not.Defined("Contributor id"), nameof(command.ContributorId));
                    return;
                }

                var user = await users.FindByIdOrEmailAsync(command.ContributorId);

                if (user == null)
                {
                    throw new DomainObjectNotFoundException(command.ContributorId, "Contributors", typeof(IAppEntity));
                }

                command.ContributorId = user.Id;

                if (string.Equals(command.ContributorId, command.Actor?.Identifier, StringComparison.OrdinalIgnoreCase) && !command.IsRestore)
                {
                    throw new DomainForbiddenException("You cannot change your own role.");
                }

                if (contributors.TryGetValue(command.ContributorId, out var existing))
                {
                    if (existing == command.Role)
                    {
                        e(Not.New("Contributor", "role"), nameof(command.Role));
                    }
                }
                else if (plan.MaxContributors == contributors.Count)
                {
                    e("You have reached the maximum number of contributors for your plan.");
                }
            });
        }

        public static void CanRemove(AppContributors contributors, RemoveContributor command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot remove contributor.", e =>
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
                throw new DomainObjectNotFoundException(command.ContributorId, "Contributors", typeof(IAppEntity));
            }
        }
    }
}
