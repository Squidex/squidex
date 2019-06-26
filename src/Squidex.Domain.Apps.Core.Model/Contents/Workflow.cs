// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class Workflow
    {
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
            Guard.NotNull(steps, nameof(steps));

            Steps = steps;

            Initial = initial;
        }

        public static Workflow Create(IReadOnlyDictionary<Status, WorkflowStep> steps, Status initial)
        {
            Guard.NotNull(steps, nameof(steps));

            foreach (var step in steps.Values)
            {
                foreach (var transition in step.Transitions)
                {
                    if (steps.ContainsKey(transition.Key))
                    {
                        throw new ArgumentException("Transitions ends to an unknown step.", nameof(initial));
                    }
                }
            }

            if (steps.ContainsKey(initial))
            {
                throw new ArgumentException("Initial step not known.", nameof(initial));
            }

            return new Workflow(steps, initial);
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

        public WorkflowTransition GetTransition(Status from, Status to)
        {
            if (TryGetStep(from, out var step) && step.Transitions.TryGetValue(to, out var transition))
            {
                return transition;
            }

            return null;
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
