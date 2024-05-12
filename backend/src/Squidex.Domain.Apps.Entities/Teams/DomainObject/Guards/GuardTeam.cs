// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject.Guards;

public static class GuardTeam
{
    public static void CanCreate(CreateTeam command)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                e(Not.Defined(nameof(command.Name)), nameof(command.Name));
            }
        });
    }

    public static void CanUpdate(UpdateTeam command)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                e(Not.Defined(nameof(command.Name)), nameof(command.Name));
            }
        });
    }

    public static void CanChangePlan(ChangePlan command, IBillingPlans billingPlans)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.PlanId))
            {
                e(Not.Defined(nameof(command.PlanId)), nameof(command.PlanId));
                return;
            }

            if (billingPlans.GetPlan(command.PlanId) == null)
            {
                e(T.Get("apps.plans.notFound"), nameof(command.PlanId));
            }
        });
    }

    public static Task CanUpsertAuth(UpsertAuth command, IAppProvider appProvider,
        CancellationToken ct)
    {
        Guard.NotNull(command);

        var scheme = command.Scheme;

        if (scheme == null)
        {
            return Task.CompletedTask;
        }

        return Validate.It(async e =>
        {
            var prefix = nameof(command.Scheme);

            if (string.IsNullOrWhiteSpace(scheme.Domain))
            {
                e(Not.Defined(nameof(scheme.Domain)), $"{prefix}.{nameof(scheme.Domain)}");
            }
            else
            {
                var existing = await appProvider.GetTeamByAuthDomainAsync(scheme.Domain, ct);

                if (existing != null && existing.Id != command.TeamId)
                {
                    e(T.Get("teams.domainInUse"));
                }
            }

            if (string.IsNullOrWhiteSpace(scheme.DisplayName))
            {
                e(Not.Defined(nameof(scheme.DisplayName)), $"{prefix}.{nameof(scheme.DisplayName)}");
            }

            if (string.IsNullOrWhiteSpace(scheme.ClientId))
            {
                e(Not.Defined(nameof(scheme.ClientId)), $"{prefix}.{nameof(scheme.ClientId)}");
            }

            if (string.IsNullOrWhiteSpace(scheme.ClientSecret))
            {
                e(Not.Defined(nameof(scheme.ClientSecret)), $"{prefix}.{nameof(scheme.ClientSecret)}");
            }

            if (string.IsNullOrWhiteSpace(scheme.Authority))
            {
                e(Not.Defined(nameof(scheme.Authority)), $"{prefix}.{nameof(scheme.Authority)}");
            }
            else if (!Uri.IsWellFormedUriString(scheme.Authority, UriKind.Absolute))
            {
                e(Not.ValidUrl(nameof(scheme.Authority)), $"{prefix}.{nameof(scheme.Authority)}");
            }

            if (!string.IsNullOrWhiteSpace(scheme.SignoutRedirectUrl) &&
                !Uri.IsWellFormedUriString(scheme.SignoutRedirectUrl, UriKind.Absolute))
            {
                e(Not.ValidUrl(nameof(scheme.SignoutRedirectUrl)), $"{prefix}.{nameof(scheme.SignoutRedirectUrl)}");
            }
        });
    }

    public static Task CanDelete(DeleteTeam command, IAppProvider appProvider,
        CancellationToken ct)
    {
        return Validate.It(async e =>
        {
            var assignedApps = await appProvider.GetTeamAppsAsync(command.TeamId, ct);

            if (assignedApps.Count != 0)
            {
                e(T.Get("teams.appsAssigned"));
            }
        });
    }
}
