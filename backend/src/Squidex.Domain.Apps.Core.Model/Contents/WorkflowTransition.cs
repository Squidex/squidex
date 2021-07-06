// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed record WorkflowTransition : WorkflowCondition
    {
        public static readonly WorkflowTransition Always = new WorkflowTransition();

        public static WorkflowTransition When(string? expression, params string[]? roles)
        {
            if (roles?.Length > 0)
            {
                return new WorkflowTransition { Expression = expression, Roles = roles?.ToImmutableList() };
            }

            if (!string.IsNullOrWhiteSpace(expression))
            {
                return new WorkflowTransition { Expression = expression };
            }

            return Always;
        }
    }
}
