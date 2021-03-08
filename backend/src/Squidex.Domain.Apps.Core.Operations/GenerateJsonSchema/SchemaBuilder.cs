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

        public static JsonSchema String()
        {
            return new JsonSchema { Type = JsonObjectType.String };
        }

        public static JsonSchemaProperty ArrayProperty(JsonSchema item, string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Array, Item = item }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty BooleanProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Boolean }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty DateTimeProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.DateTime }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty NumberProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Number }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty StringProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.String }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty ReferenceProperty(JsonSchema reference, string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Reference = reference }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty ObjectProperty(JsonSchema? value = null, string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Object, AdditionalPropertiesSchema = value }
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static JsonSchemaProperty JsonProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty()
                .SetDescription(description)
                .SetRequired(isRequired);
        }

        public static T SetDescription<T>(this T property, string? description = null) where T : JsonSchemaProperty
        {
            property.Description = description;

            return property;
        }

        public static T SetRequired<T>(this T property, bool isRequired) where T : JsonSchemaProperty
        {
            property.IsRequired = isRequired;
            property.IsNullableRaw = !isRequired;

            return property;
        }
    }
}
