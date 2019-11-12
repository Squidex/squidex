// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public class JsonWorkflowTransition
    {
        [JsonProperty]
        public string Expression { get; set; }

        [JsonProperty]
        public string Role { get; set; }

        [JsonProperty]
        public string[] Roles { get; }

        public JsonWorkflowTransition()
        {
        }

        public JsonWorkflowTransition(WorkflowTransition transition)
        {
            SimpleMapper.Map(transition, this);
        }

        public WorkflowTransition ToTransition()
        {
            var roles = Roles;

            if (!string.IsNullOrEmpty(Role))
            {
                roles = new[] { Role };
            }

            return new WorkflowTransition(Expression, roles);
        }
    }
}