// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class JsonWorkflowStep
    {
        [JsonProperty]
        public Dictionary<Status, JsonWorkflowTransition> Transitions { get; set; }

        [JsonProperty]
        public string? Color { get; set; }

        [JsonProperty("noUpdate")]
        public bool NoUpdateFlag { get; set; }

        [JsonProperty("noUpdateRules")]
        public NoUpdate? NoUpdate { get; set; }

        public JsonWorkflowStep()
        {
        }

        public JsonWorkflowStep(WorkflowStep step)
        {
            SimpleMapper.Map(step, this);

            Transitions =
                step.Transitions.ToDictionary(
                    x => x.Key,
                    x => new JsonWorkflowTransition(x.Value));
        }

        public WorkflowStep ToStep()
        {
            var noUpdate = NoUpdate;

            if (NoUpdateFlag)
            {
                noUpdate = NoUpdate.Always;
            }

            var transitions =
                Transitions?.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToTransition());

            return new WorkflowStep(transitions, Color, noUpdate);
        }
    }
}
