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
    public sealed class WorkflowStepConverter : JsonClassConverter<WorkflowStep>
    {
        protected override void WriteValue(JsonWriter writer, WorkflowStep value, JsonSerializer serializer)
        {
            var json = new JsonWorkflowStep(value);

            serializer.Serialize(writer, json);
        }

        protected override WorkflowStep ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<JsonWorkflowStep>(reader)!;

            return json.ToStep();
        }
    }
}
