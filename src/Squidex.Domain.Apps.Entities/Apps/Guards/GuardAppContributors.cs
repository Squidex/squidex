// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security;
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
        public static Task CanAssign(AppContributors contributors, AssignContributor command, IUserResolver users, IAppLimitsPlan plan)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot assign contributor.", async e =>
            {
                if (!command.Permission.IsEnumValue())
                {
                    e("Permission is not valid.", nameof(command.Permission));
                }

                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    e("Contributor id is required.", nameof(command.ContributorId));
                    return;
                }

                var user = await users.FindByIdOrEmailAsync(command.ContributorId);

                if (user == null)
                {
                    throw new DomainObjectNotFoundException(command.ContributorId, "Contributors", typeof(IAppEntity));
                }

                command.ContributorId = user.Id;

                if (string.Equals(command.ContributorId, command.Actor?.Identifier, StringComparison.OrdinalIgnoreCase) && !command.FromRestore)
                {
                    throw new SecurityException("You cannot change your own permission.");
                }

                if (contributors.TryGetValue(command.ContributorId, out var existing))
                {
                    if (existing == command.Permission)
                    {
                        e("Contributor has already this permission.", nameof(command.Permission));
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
                    e("Contributor id is required.", nameof(command.ContributorId));
                }

                var ownerIds = contributors.Where(x => x.Value == AppContributorPermission.Owner).Select(x => x.Key).ToList();

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
