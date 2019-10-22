// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowTransition
    {
        public static readonly WorkflowTransition Default = new WorkflowTransition();

        public string Expression { get; }

        public IReadOnlyList<string> Roles { get; }

        public WorkflowTransition(string expression = null, IReadOnlyList<string> roles = null)
        {
            Expression = expression;

            Roles = roles;
        }
    }
}