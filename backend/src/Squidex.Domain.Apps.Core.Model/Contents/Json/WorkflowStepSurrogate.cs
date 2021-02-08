// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    [DataContract]
    public sealed class WorkflowStepSurrogate : ISurrogate<WorkflowStep>
    {
        [DataMember(Name = "transitions")]
        public Dictionary<Status, WorkflowTransitionSurrogate> Transitions { get; set; }

        [DataMember(Name = "noUpdate")]
        public bool NoUpdateFlag { get; set; }

        [DataMember(Name = "noUpdateRules")]
        public NoUpdate? NoUpdate { get; set; }

        [DataMember(Name = "color")]
        public string? Color { get; set; }

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

            if (NoUpdateFlag)
            {
                noUpdate = NoUpdate.Always;
            }

            var transitions =
                Transitions?.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToSource());

            return new WorkflowStep(transitions, Color, noUpdate);
        }
    }
}
