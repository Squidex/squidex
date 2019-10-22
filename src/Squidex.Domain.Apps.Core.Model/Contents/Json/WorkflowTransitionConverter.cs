// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class WorkflowTransitionConverter : JsonClassConverter<WorkflowTransition>
    {
        protected override void WriteValue(JsonWriter writer, WorkflowTransition value, JsonSerializer serializer)
        {
            var json = new JsonWorkflowTransition(value);

            serializer.Serialize(writer, json);
        }

        protected override WorkflowTransition ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<JsonWorkflowTransition>(reader);

            return json.ToTransition();
        }
    }
}
