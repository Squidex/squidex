// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class SchemaBuilder
    {
        public static JsonSchema Object()
        {
            return new JsonSchema { Type = JsonObjectType.Object };
        }

        public static JsonSchema Guid()
        {
            return new JsonSchema { Type = JsonObjectType.String, Format = JsonFormatStrings.Guid };
        }

        public static JsonSchema String()
        {
            return new JsonSchema { Type = JsonObjectType.String };
        }

        public static JsonSchemaProperty ArrayProperty(JsonSchema item, string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.Array, Item = item }, description, isRequired);
        }

        public static JsonSchemaProperty BooleanProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.Boolean }, description, isRequired);
        }

        public static JsonSchemaProperty DateTimeProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.DateTime }, description, isRequired);
        }

        public static JsonSchemaProperty GuidProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Guid }, description, isRequired);
        }

        public static JsonSchemaProperty NumberProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.Number }, description, isRequired);
        }

        public static JsonSchemaProperty ObjectProperty(JsonSchema item, string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.Object, Reference = item }, description, isRequired);
        }

        public static JsonSchemaProperty StringProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty { Type = JsonObjectType.String }, description, isRequired);
        }

        public static JsonSchemaProperty JsonProperty(string? description = null, bool isRequired = false)
        {
            return Enrich(new JsonSchemaProperty(), description, isRequired);
        }

        private static JsonSchemaProperty Enrich(JsonSchemaProperty property, string? description = null, bool isRequired = false)
        {
            property.Description = description;
            property.SetRequired(isRequired);

            return property;
        }

        public static void SetRequired(this JsonSchemaProperty property, bool isRequired)
        {
            if (isRequired)
            {
                property.IsRequired = true;
            }
            else
            {
                property.IsNullableRaw = true;
            }
        }
    }
}
