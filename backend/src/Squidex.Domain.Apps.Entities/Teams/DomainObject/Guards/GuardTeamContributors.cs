// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;

public static class GuardTeamContributors
{
    public static Task CanAssign(AssignContributor command, ITeamEntity team, IUserResolver users)
    {
        Guard.NotNull(command);

        var contributors = team.Contributors;

        return Validate.It(async e =>
        {
            if (command.Role != Role.Owner)
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

                if (!command.IgnoreActor && string.Equals(command.ContributorId, command.Actor?.Identifier, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DomainForbiddenException(T.Get("apps.contributors.cannotChangeYourself"));
                }
            }
        });
    }

    public static void CanRemove(RemoveContributor command, ITeamEntity team)
    {
        Guard.NotNull(command);

        var contributors = team.Contributors;

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
