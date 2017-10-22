// ==========================================================================
//  GuardApp.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Write.Apps.Guards
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
                    if (await users.FindByIdAsync(command.ContributorId) == null)
                    {
                        error(new ValidationError("Cannot find contributor id.", nameof(command.ContributorId)));
                    }
                    else if (contributors.Contributors.TryGetValue(command.ContributorId, out var existing))
                    {
                        if (existing == command.Permission)
                        {
                            error(new ValidationError("Contributor has already this permission.", nameof(command.Permission)));
                        }
                    }
                    else if (plan.MaxContributors == contributors.Contributors.Count)
                    {
                        error(new ValidationError("You have reached the maximum number of contributors for your plan."));
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

                var ownerIds = contributors.Contributors.Where(x => x.Value == AppContributorPermission.Owner).Select(x => x.Key).ToList();

                if (ownerIds.Count == 1 && ownerIds.Contains(command.ContributorId))
                {
                    error(new ValidationError("Cannot remove the only owner.", nameof(command.ContributorId)));
                }
            });

            if (!contributors.Contributors.ContainsKey(command.ContributorId))
            {
                throw new DomainObjectNotFoundException(command.ContributorId, "Contributors", typeof(AppDomainObject));
            }
        }
    }
}
