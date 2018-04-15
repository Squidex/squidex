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
        public static Task CanAssign(AppContributors contributors, AssignContributor command, IUserResolver users, IAppLimitsPlan plan)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot assign contributor.", async error =>
            {
                if (!command.Permission.IsEnumValue())
                {
                    error(new ValidationError("Permission is not valid.", nameof(command.Permission)));
                }

                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    error(new ValidationError("Contributor id not assigned.", nameof(command.ContributorId)));
                }
                else
                {
                    var user = await users.FindByIdOrEmailAsync(command.ContributorId);

                    if (user == null)
                    {
                        error(new ValidationError("Cannot find contributor id.", nameof(command.ContributorId)));
                    }
                    else
                    {
                        command.ContributorId = user.Id;

                        if (string.Equals(command.ContributorId, command.Actor?.Identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            error(new ValidationError("You cannot change your own permission."));
                        }
                        else if (contributors.TryGetValue(command.ContributorId, out var existing))
                        {
                            if (existing == command.Permission)
                            {
                                error(new ValidationError("Contributor has already this permission.", nameof(command.Permission)));
                            }
                        }
                        else if (plan.MaxContributors == contributors.Count)
                        {
                            error(new ValidationError("You have reached the maximum number of contributors for your plan."));
                        }
                    }
                }
            });
        }

        public static void CanRemove(AppContributors contributors, RemoveContributor command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot remove contributor.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.ContributorId))
                {
                    error(new ValidationError("Contributor id not assigned.", nameof(command.ContributorId)));
                }

                var ownerIds = contributors.Where(x => x.Value == AppContributorPermission.Owner).Select(x => x.Key).ToList();

                if (ownerIds.Count == 1 && ownerIds.Contains(command.ContributorId))
                {
                    error(new ValidationError("Cannot remove the only owner.", nameof(command.ContributorId)));
                }
            });

            if (!contributors.ContainsKey(command.ContributorId))
            {
                throw new DomainObjectNotFoundException(command.ContributorId, "Contributors", typeof(IAppEntity));
            }
        }
    }
}
