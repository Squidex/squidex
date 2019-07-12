// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowTransition
    {
        public static readonly WorkflowTransition Default = new WorkflowTransition();

        public string Expression { get; }

        public string Role { get; }

        public WorkflowTransition(string expression = null, string role = null)
        {
            Expression = expression;

            Role = role;
        }
    }
}
