// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class WorkflowsConverter : JsonClassConverter<Workflows>
    {
        protected override void WriteValue(JsonWriter writer, Workflows value, JsonSerializer serializer)
        {
            var json = new Dictionary<DomainId, Workflow>(value.Count);

            foreach (var (key, workflow) in value)
            {
                json.Add(key, workflow);
            }

            serializer.Serialize(writer, json);
        }

        protected override Workflows ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<DomainId, Workflow>>(reader);

            return new Workflows(json!);
        }
    }
}
