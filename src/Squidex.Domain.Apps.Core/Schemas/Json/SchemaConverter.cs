// ==========================================================================
//  SchemaConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class SchemaConverter : JsonConverter
    {
        private readonly FieldRegistry fieldRegistry;

        public SchemaConverter(FieldRegistry fieldRegistry)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));

            this.fieldRegistry = fieldRegistry;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new JsonSchemaModel((Schema)value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<JsonSchemaModel>(reader).ToSchema(fieldRegistry);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Schema);
        }
    }
}