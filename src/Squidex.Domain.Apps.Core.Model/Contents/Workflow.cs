// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class Workflow : Named
    {
        private const string DefaultName = "Unnamed";

        public static readonly IReadOnlyDictionary<Status, WorkflowStep> EmptySteps = new Dictionary<Status, WorkflowStep>();
        public static readonly IReadOnlyList<Guid> EmptySchemaIds = new List<Guid>();
        public static readonly Workflow Default = CreateDefault();
        public static readonly Workflow Empty = new Workflow(default, EmptySteps);

        public IReadOnlyDictionary<Status, WorkflowStep> Steps { get; } = EmptySteps;

        public IReadOnlyList<Guid> SchemaIds { get; } = EmptySchemaIds;

        public Status Initial { get; }

        public Workflow(
            Status initial,
            IReadOnlyDictionary<Status, WorkflowStep> steps,
            IReadOnlyList<Guid> schemaIds = null,
            string name = null)
            : base(name ?? DefaultName)
        {
            Initial = initial;

            if (steps != null)
            {
                Steps = steps;
            }

            if (schemaIds != null)
            {
                SchemaIds = schemaIds;
            }
        }

        public static Workflow CreateDefault(string name = null)
        {
            return new Workflow(
                Status.Draft, new Dictionary<Status, WorkflowStep>
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
                                [Status.Draft] = new WorkflowTransition()
                            },
                            StatusColors.Published)
                }, null, name);
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
            else if (TryGetStep(Initial, out var initial))
            {
                yield return (Initial, initial, WorkflowTransition.Default);
            }
        }

        public bool TryGetTransition(Status from, Status to, out WorkflowTransition transition)
        {
            transition = null;

            if (TryGetStep(from, out var step))
            {
                if (step.Transitions.TryGetValue(to, out transition))
                {
                    return true;
                }
            }
            else if (to == Initial)
            {
                transition = WorkflowTransition.Default;

                return true;
            }

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
