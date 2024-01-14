// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class RequiredSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.GetAttribute<OpenApiRequestAttribute>(true) != null)
        {
            FixRequest(context);
        }
        else
        {
            FixResponse(context);
        }
    }

    private static void FixRequest(SchemaProcessorContext context)
    {
        FixRequired(context.Schema);

        foreach (var schema in context.Schema.AllOf)
        {
            FixRequired(schema);
        }

        foreach (var schema in context.Schema.OneOf)
        {
            FixRequired(schema);
        }

        static void FixRequired(JsonSchema schema)
        {
            foreach (var property in schema.Properties.Values)
            {
                if (IsValueType(property) || property.IsEnumeration)
                {
                    property.IsRequired = false;
                }
            }
        }

        static bool IsValueType(JsonSchemaProperty property)
        {
            return property.Type is JsonObjectType.Boolean or JsonObjectType.Integer or JsonObjectType.Number;
        }
    }

    private static void FixResponse(SchemaProcessorContext context)
    {
        FixRequired(context.Schema);

        foreach (var schema in context.Schema.AllOf)
        {
            FixRequired(schema);
        }

        foreach (var schema in context.Schema.OneOf)
        {
            FixRequired(schema);
        }

        static void FixRequired(JsonSchema schema)
        {
            foreach (var property in schema.Properties.Values)
            {
                if (!property.IsNullable(SchemaType.OpenApi3))
                {
                    property.IsRequired = true;
                }
            }
        }
    }
}
