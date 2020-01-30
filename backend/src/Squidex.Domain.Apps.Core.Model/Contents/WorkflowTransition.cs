// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class WorkflowTransition : WorkflowCondition
    {
        public static readonly WorkflowTransition Always = new WorkflowTransition(null, null);

        public WorkflowTransition(string? expression, ReadOnlyCollection<string>? roles)
            : base(expression, roles)
        {
        }

        public static WorkflowTransition When(string? expression, params string[]? roles)
        {
            if (roles?.Length > 0)
            {
                return new WorkflowTransition(expression, ReadOnlyCollection.Create(roles));
            }
            else if (!string.IsNullOrWhiteSpace(expression))
            {
                return new WorkflowTransition(expression, null);
            }
            else
            {
                return Always;
            }
        }
    }
}