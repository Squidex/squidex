// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowStep
    {
        public IReadOnlyDictionary<Status, WorkflowTransition> Transitions { get; }

        public string Color { get; }

        public bool NoUpdate { get; }

        public WorkflowStep(IReadOnlyDictionary<Status, WorkflowTransition> transitions, string color, bool noUpdate = false)
        {
            Guard.NotNull(transitions, nameof(transitions));

            Transitions = transitions;

            Color = color;

            NoUpdate = noUpdate;
        }
    }
}
