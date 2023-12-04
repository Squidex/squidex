// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Contents.Json;

public sealed class WorkflowStepSurrogate : ISurrogate<WorkflowStep>
{
    [Obsolete("Old serialization format.")]
    private bool noUpdateFlag;

    [JsonPropertyName("noUpdateRules")]
    public NoUpdate? NoUpdate { get; set; }

    [JsonPropertyName("noUpdate")]
    [Obsolete("Old serialization format.")]
    public bool NoUpdateFlag
    {
        // Because this property is old we old want to read it and never to write it.
        set => noUpdateFlag = value;
    }

    public bool Validate { get; set; }

    public string? Color { get; set; }

    public Dictionary<Status, WorkflowTransitionSurrogate> Transitions { get; set; }

    public void FromSource(WorkflowStep source)
    {
        SimpleMapper.Map(source, this);

        Transitions = source.Transitions.ToDictionary(x => x.Key, source =>
        {
            var surrogate = new WorkflowTransitionSurrogate();

            surrogate.FromSource(source.Value);

            return surrogate;
        });
    }

    public WorkflowStep ToSource()
    {
        var noUpdate = NoUpdate;

#pragma warning disable CS0618 // Type or member is obsolete
        // The flag has been replaced with an object.
        if (noUpdateFlag)
        {
            noUpdate = NoUpdate.Always;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        var transitions =
            Transitions?.ToReadonlyDictionary(
                x => x.Key,
                x => x.Value.ToSource());

        return new WorkflowStep(transitions, Color, noUpdate, Validate);
    }
}
