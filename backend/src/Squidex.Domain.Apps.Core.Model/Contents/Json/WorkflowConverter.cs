// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class WorkflowConverter : JsonClassConverter<Workflows>
    {
        protected override void WriteValue(JsonWriter writer, Workflows value, JsonSerializer serializer)
        {
            var json = new Dictionary<Guid, Workflow>(value.Count);

            foreach (var workflow in value)
            {
                json.Add(workflow.Key, workflow.Value);
            }

            serializer.Serialize(writer, json);
        }

        protected override Workflows ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<Guid, Workflow>>(reader);

            return new Workflows(json.ToArray());
        }
    }
}
