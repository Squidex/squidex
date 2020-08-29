// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;

#pragma warning disable IDE0051 // Remove unused private members

namespace Squidex.Domain.Apps.Core.Contents
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class Workflow : Named
    {
        private const string DefaultName = "Unnamed";

        public static readonly IReadOnlyDictionary<Status, WorkflowStep> EmptySteps = new Dictionary<Status, WorkflowStep>();
        public static readonly IReadOnlyList<DomainId> EmptySchemaIds = new List<DomainId>();
        public static readonly Workflow Default = CreateDefault();
        public static readonly Workflow Empty = new Workflow(default, EmptySteps);

        [IgnoreDuringEquals]
        public IReadOnlyDictionary<Status, WorkflowStep> Steps { get; } = EmptySteps;

        public IReadOnlyList<DomainId> SchemaIds { get; } = EmptySchemaIds;

        public Status Initial { get; }

        public Workflow(
            Status initial,
            IReadOnlyDictionary<Status, WorkflowStep>? steps,
            IReadOnlyList<DomainId>? schemaIds = null,
            string? name = null)
            : base(name.Or(DefaultName))
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

        public static Workflow CreateDefault(string? name = null)
        {
            return new Workflow(
                Status.Draft, new Dictionary<Status, WorkflowStep>
                {
                    [Status.Archived] =
                        new WorkflowStep(
                            new Dictionary<Status, WorkflowTransition>
                            {
                                [Status.Draft] = WorkflowTransition.Always
                            },
                            StatusColors.Archived, NoUpdate.Always),
                    [Status.Draft] =
                        new WorkflowStep(
                            new Dictionary<Status, WorkflowTransition>
                            {
                                [Status.Archived] = WorkflowTransition.Always,
                                [Status.Published] = WorkflowTransition.Always
                            },
                            StatusColors.Draft),
                    [Status.Published] =
                        new WorkflowStep(
                            new Dictionary<Status, WorkflowTransition>
                            {
                                [Status.Archived] = WorkflowTransition.Always,
                                [Status.Draft] = WorkflowTransition.Always
                            },
                            StatusColors.Published)
                }, null, name);
        }

        public IEnumerable<(Status Status, WorkflowStep Step, WorkflowTransition Transition)> GetTransitions(Status status)
        {
            if (TryGetStep(status, out var step))
            {
                foreach (var (nextStatus, transition) in step.Transitions)
                {
                    yield return (nextStatus, Steps[nextStatus], transition);
                }
            }
            else if (TryGetStep(Initial, out var initial))
            {
                yield return (Initial, initial, WorkflowTransition.Always);
            }
        }

        public bool TryGetTransition(Status from, Status to, [MaybeNullWhen(false)] out WorkflowTransition transition)
        {
            transition = null!;

            if (TryGetStep(from, out var step))
            {
                if (step.Transitions.TryGetValue(to, out transition!))
                {
                    return true;
                }
            }
            else if (to == Initial)
            {
                transition = WorkflowTransition.Always;

                return true;
            }

            return false;
        }

        public bool TryGetStep(Status status, [MaybeNullWhen(false)] out WorkflowStep step)
        {
            return Steps.TryGetValue(status, out step!);
        }

        public (Status Key, WorkflowStep) GetInitialStep()
        {
            return (Initial, Steps[Initial]);
        }

        [CustomEqualsInternal]
        private bool CustomEquals(Workflow other)
        {
            return Steps.EqualsDictionary(other.Steps);
        }

        [CustomGetHashCode]
        private int CustomHashCode()
        {
            return Steps.DictionaryHashCode();
        }
    }
}
