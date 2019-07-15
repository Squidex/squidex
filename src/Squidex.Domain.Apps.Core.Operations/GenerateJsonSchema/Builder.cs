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
        public static JsonSchema4 Object()
        {
            return new JsonSchema4 { Type = JsonObjectType.Object, AllowAdditionalItems = false };
        }

        public static JsonSchema4 Guid()
        {
            return new JsonSchema4 { Type = JsonObjectType.String, Format = JsonFormatStrings.Guid };
        }

        public static JsonSchema4 String()
        {
            return new JsonSchema4 { Type = JsonObjectType.String };
        }

        public static JsonProperty ArrayProperty(JsonSchema4 item)
        {
            return new JsonProperty { Type = JsonObjectType.Array, Item = item };
        }

        public static JsonProperty BooleanProperty()
        {
            return new JsonProperty { Type = JsonObjectType.Boolean };
        }

        public static JsonProperty DateTimeProperty(string description = null, bool isRequired = false)
        {
            return new JsonProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.DateTime, Description = description, IsRequired = isRequired };
        }

        public static JsonProperty GuidProperty(string description = null, bool isRequired = false)
        {
            return new JsonProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Guid, Description = description, IsRequired = isRequired };
        }

        public static JsonProperty NumberProperty(string description = null, bool isRequired = false)
        {
            return new JsonProperty { Type = JsonObjectType.Number, Description = description, IsRequired = isRequired };
        }

        public static JsonProperty ObjectProperty(JsonSchema4 item, string description = null, bool isRequired = false)
        {
            return new JsonProperty { Type = JsonObjectType.Object, Reference = item, Description = description, IsRequired = isRequired };
        }

        public static JsonProperty StringProperty(string description = null, bool isRequired = false)
        {
            return new JsonProperty { Type = JsonObjectType.String, Description = description, IsRequired = isRequired };
        }
    }
}
