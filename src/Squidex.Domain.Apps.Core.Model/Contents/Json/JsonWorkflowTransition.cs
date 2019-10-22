// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
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
        public IReadOnlyList<string> Roles { get; }

        public JsonWorkflowTransition()
        {
        }

        public JsonWorkflowTransition(WorkflowTransition client)
        {
            SimpleMapper.Map(client, this);
        }

        public WorkflowTransition ToTransition()
        {
            var roles = Roles;

            if (!string.IsNullOrEmpty(Role))
            {
                roles = new List<string> { Role };
            }

            return new WorkflowTransition(Expression, roles);
        }
    }
}
