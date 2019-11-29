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
    public abstract class WorkflowCondition
    {
        public string? Expression { get; }

        public ReadOnlyCollection<string>? Roles { get; }

        protected WorkflowCondition(string? expression, params string[]? roles)
        {
            Expression = expression;

            if (roles != null)
            {
                Roles = ReadOnlyCollection.Create(roles);
            }
        }
    }
}