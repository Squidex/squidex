// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class Builder
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

        public static JsonSchemaProperty ArrayProperty(JsonSchema item)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Array, Item = item };
        }

        public static JsonSchemaProperty BooleanProperty()
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Boolean };
        }

        public static JsonSchemaProperty DateTimeProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.DateTime, Description = description, IsRequired = isRequired };
        }

        public static JsonSchemaProperty GuidProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Guid, Description = description, IsRequired = isRequired };
        }

        public static JsonSchemaProperty NumberProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Number, Description = description, IsRequired = isRequired };
        }

        public static JsonSchemaProperty ObjectProperty(JsonSchema item, string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.Object, Reference = item, Description = description, IsRequired = isRequired };
        }

        public static JsonSchemaProperty StringProperty(string? description = null, bool isRequired = false)
        {
            return new JsonSchemaProperty { Type = JsonObjectType.String, Description = description, IsRequired = isRequired };
        }
    }
}
