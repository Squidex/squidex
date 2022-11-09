// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Contents;

public sealed record Workflow(Status Initial, ReadonlyDictionary<Status, WorkflowStep>? Steps = null, ReadonlyList<DomainId>? SchemaIds = null, string? Name = null)
{
    private const string DefaultName = "Unnamed";

    public static readonly Workflow Default = CreateDefault();
    public static readonly Workflow Empty = new Workflow(default, null);

    public string Name { get; } = Name.Or(DefaultName);

    public ReadonlyDictionary<Status, WorkflowStep> Steps { get; } = Steps ?? ReadonlyDictionary.Empty<Status, WorkflowStep>();

    public ReadonlyList<DomainId> SchemaIds { get; } = SchemaIds ?? ReadonlyList.Empty<DomainId>();

    public static Workflow CreateDefault(string? name = null)
    {
        return new Workflow(
            Status.Draft,
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Archived] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Draft] = WorkflowTransition.Always
                        }.ToReadonlyDictionary(),
                        StatusColors.Archived, NoUpdate.Always),
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = WorkflowTransition.Always,
                            [Status.Published] = WorkflowTransition.Always
                        }.ToReadonlyDictionary(),
                        StatusColors.Draft),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = WorkflowTransition.Always,
                            [Status.Draft] = WorkflowTransition.Always
                        }.ToReadonlyDictionary(),
                        StatusColors.Published)
            }.ToReadonlyDictionary(), null, name);
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
}
