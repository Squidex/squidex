// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class Workflow
    {
        private static readonly IReadOnlyDictionary<Status, WorkflowStep> EmptySteps = new Dictionary<Status, WorkflowStep>();

        public static readonly Workflow Default = new Workflow(
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Archived] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Draft] = new WorkflowTransition()
                        },
                        StatusColors.Archived, true),
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = new WorkflowTransition(),
                            [Status.Published] = new WorkflowTransition()
                        },
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = new WorkflowTransition(),
                            [Status.Published] = new WorkflowTransition()
                        },
                        StatusColors.Archived)
            }, Status.Draft);

        public IReadOnlyDictionary<Status, WorkflowStep> Steps { get; }

        public Status Initial { get; }

        public Workflow(IReadOnlyDictionary<Status, WorkflowStep> steps, Status initial)
        {
            Steps = steps ?? EmptySteps;

            Initial = initial;
        }

        public IEnumerable<(Status Status, WorkflowStep Step, WorkflowTransition Transition)> GetTransitions(Status status)
        {
            if (TryGetStep(status, out var step))
            {
                foreach (var transition in step.Transitions)
                {
                    yield return (transition.Key, Steps[transition.Key], transition.Value);
                }
            }
        }

        public bool TryGetTransition(Status from, Status to, out WorkflowTransition transition)
        {
            if (TryGetStep(from, out var step) && step.Transitions.TryGetValue(to, out transition))
            {
                return true;
            }

            transition = null;

            return false;
        }

        public bool TryGetStep(Status status, out WorkflowStep step)
        {
            return Steps.TryGetValue(status, out step);
        }

        public (Status Key, WorkflowStep) GetInitialStep()
        {
            return (Initial, Steps[Initial]);
        }
    }
}
