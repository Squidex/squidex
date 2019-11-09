// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class SchemaConverter : JsonClassConverter<Schema>
    {
        protected override void WriteValue(JsonWriter writer, Schema value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JsonSchemaModel(value));
        }

        protected override Schema ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            return serializer.Deserialize<JsonSchemaModel>(reader)!.ToSchema();
        }
    }
}