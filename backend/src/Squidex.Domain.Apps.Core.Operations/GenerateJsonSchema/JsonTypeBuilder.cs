// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema;

public static class JsonTypeBuilder
{
    public static JsonSchema Object()
    {
        const JsonObjectType type = JsonObjectType.Object;

        return new JsonSchema { Type = type, AllowAdditionalProperties = false };
    }

    public static JsonSchema String()
    {
        const JsonObjectType type = JsonObjectType.String;

        return new JsonSchema { Type = type };
    }

    public static JsonSchemaProperty BooleanProperty(string? description = null, bool isRequired = false)
    {
        const JsonObjectType type = JsonObjectType.Boolean;

        return new JsonSchemaProperty { Type = type }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty DateTimeProperty(string? description = null, bool isRequired = false)
    {
        const JsonObjectType type = JsonObjectType.String;

        return new JsonSchemaProperty { Type = type, Format = JsonFormatStrings.DateTime }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty NumberProperty(string? description = null, bool isRequired = false)
    {
        const JsonObjectType type = JsonObjectType.Number;

        return new JsonSchemaProperty { Type = type }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty StringProperty(string? description = null, bool isRequired = false, string? format = null)
    {
        const JsonObjectType type = JsonObjectType.String;

        return new JsonSchemaProperty { Type = type, Format = format }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty ObjectProperty(JsonSchema? value = null, string? description = null, bool isRequired = false)
    {
        const JsonObjectType type = JsonObjectType.Object;

        return new JsonSchemaProperty { Type = type, AdditionalPropertiesSchema = value }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty ArrayProperty(JsonSchema item, string? description = null, bool isRequired = false)
    {
        const JsonObjectType type = JsonObjectType.Array;

        return new JsonSchemaProperty { Type = type, Item = item }
            .SetDescription(description)
            .SetRequired(isRequired);
    }

    public static JsonSchemaProperty ReferenceProperty(JsonSchema reference, string? description = null, bool isRequired = false)
    {
        return new JsonSchemaProperty { Reference = reference }
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
