// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class WorkflowStep
    {
        private static readonly IReadOnlyDictionary<Status, WorkflowTransition> EmptyTransitions = new Dictionary<Status, WorkflowTransition>();

        public IReadOnlyDictionary<Status, WorkflowTransition> Transitions { get; }

        public string Color { get; }

        public bool NoUpdate { get; }

        public WorkflowStep(IReadOnlyDictionary<Status, WorkflowTransition> transitions = null, string color = null, bool noUpdate = false)
        {
            Transitions = transitions ?? EmptyTransitions;

            Color = color;

            NoUpdate = noUpdate;
        }
    }
}
