// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowTransition : WorkflowCondition
    {
        public static readonly WorkflowTransition Always = new WorkflowTransition(null, null);

        public WorkflowTransition(string? expression, params string[]? roles)
            : base(expression, roles)
        {
        }

        public static WorkflowTransition When(string? expression, params string[]? roles)
        {
            return new WorkflowTransition(expression, roles);
        }
    }
}