// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Squidex.Infrastructure.Collections;
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
        public List<string> Roles { get; }

        public JsonWorkflowTransition()
        {
        }

        public JsonWorkflowTransition(WorkflowTransition client)
        {
            SimpleMapper.Map(client, this);
        }

        public WorkflowTransition ToTransition()
        {
            var rolesList = Roles;

            if (!string.IsNullOrEmpty(Role))
            {
                rolesList = new List<string> { Role };
            }

            ReadOnlyCollection<string> roles = null;

            if (rolesList != null && rolesList.Count > 0)
            {
                roles = ReadOnlyCollection.Create(rolesList.ToArray());
            }

            return new WorkflowTransition(Expression, roles);
        }
    }
}
