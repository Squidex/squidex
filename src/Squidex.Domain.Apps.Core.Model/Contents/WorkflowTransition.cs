// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowTransition
    {
        public static readonly WorkflowTransition Default = new WorkflowTransition();

        public string Expression { get; }

        public ReadOnlyCollection<string> Roles { get; }

        public WorkflowTransition(string expression = null, ReadOnlyCollection<string> roles = null)
        {
            Expression = expression;

            Roles = roles;
        }
    }
}