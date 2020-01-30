// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Newtonsoft.Json;
namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public class JsonWorkflowTransition
    {
        [JsonProperty]
        public string? Expression { get; set; }

        [JsonProperty]
        public string? Role { get; set; }

        [JsonProperty]
        public string[]? Roles { get; set; }

        public JsonWorkflowTransition()
        {
        }

        public JsonWorkflowTransition(WorkflowTransition transition)
        {
            Roles = transition.Roles?.ToArray();

            Expression = transition.Expression;
        }

        public WorkflowTransition ToTransition()
        {
            var roles = Roles;

            if (!string.IsNullOrEmpty(Role))
            {
                roles = new[] { Role };
            }

            return WorkflowTransition.When(Expression, roles);
        }
    }
}