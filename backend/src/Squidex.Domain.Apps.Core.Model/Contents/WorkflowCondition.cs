// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Contents
{
    public abstract class WorkflowCondition
    {
        public string? Expression { get; }

        public ReadOnlyCollection<string>? Roles { get; }

        protected WorkflowCondition(string? expression, ReadOnlyCollection<string>? roles)
        {
            Expression = expression;

            Roles = roles;
        }
    }
}