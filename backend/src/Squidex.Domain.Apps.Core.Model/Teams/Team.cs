// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Core.Teams;

public record Team : Entity
{
    public string Name { get; init; }

    public Contributors Contributors { get; init; } = Contributors.Empty;

    public AssignedPlan? Plan { get; init; }

    public AuthScheme? AuthScheme { get; init; }

    public bool IsDeleted { get; init; }

    [Pure]
    public Team Rename(string name)
    {
        Guard.NotNull(name);

        if (string.Equals(Name, name, StringComparison.Ordinal))
        {
            return this;
        }

        return this with { Name = name };
    }

    [Pure]
    public Team ChangePlan(AssignedPlan? plan)
    {
        if (Equals(plan?.PlanId, Plan?.PlanId))
        {
            return this;
        }

        return this with { Plan = plan };
    }

    [Pure]
    public Team ChangeAuthScheme(AuthScheme? authScheme)
    {
        if (Equals(authScheme, AuthScheme))
        {
            return this;
        }

        return this with { AuthScheme = authScheme };
    }

    [Pure]
    public Team UpdateContributors<T>(T state, Func<T, Contributors, Contributors> update)
    {
        var newContributors = update(state, Contributors);

        if (ReferenceEquals(newContributors, Contributors))
        {
            return this;
        }

        return this with { Contributors = newContributors };
    }
}
