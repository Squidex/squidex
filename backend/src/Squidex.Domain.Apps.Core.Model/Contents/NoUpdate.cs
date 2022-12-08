// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents;

public sealed record NoUpdate : WorkflowCondition
{
    public static readonly NoUpdate Always = new NoUpdate();

    public static NoUpdate When(string? expression, params string[]? roles)
    {
        if (roles?.Length > 0)
        {
            return new NoUpdate { Expression = expression, Roles = roles?.ToReadonlyList() };
        }

        if (!string.IsNullOrWhiteSpace(expression))
        {
            return new NoUpdate { Expression = expression };
        }

        return Always;
    }
}
