// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema;

public delegate (JsonSchema Reference, JsonSchema? Actual) JsonTypeFactory(string name);

public static class JsonSchemaExtensions
{
    private static readonly JsonTypeFactory DefaultFactory = _ =>
    {
        var schema = JsonTypeBuilder.Object();

        return (schema, schema);
    };

    public static JsonSchema BuildJsonSchemaFlat(this Schema schema, PartitionResolver partitionResolver,
        ResolvedComponents components,
        JsonTypeFactory? factory = null,
        bool withHidden = false,
        bool withComponents = false)
    {
        Guard.NotNull(partitionResolver);
        Guard.NotNull(components);

        factory ??= DefaultFactory;

        var jsonSchema = JsonTypeBuilder.Object();

        foreach (var field in schema.Fields.ForApi(withHidden))
        {
            var property =
                JsonTypeVisitor.BuildProperty(
                    field, components, schema,
                    factory,
                    withHidden,
                    withComponents);

            // Property is null for UI fields.
            if (property != null)
            {
                property.SetRequired(false);
                property.SetDescription(field);

                jsonSchema.Properties.Add(field.Name, property);
            }
        }

        return jsonSchema;
    }

    public static JsonSchema BuildJsonSchemaDynamic(this Schema schema, PartitionResolver partitionResolver,
        ResolvedComponents components,
        JsonTypeFactory? factory = null,
        bool withHidden = false,
        bool withComponents = false)
    {
        Guard.NotNull(partitionResolver);
        Guard.NotNull(components);

        factory ??= DefaultFactory;

        var jsonSchema = JsonTypeBuilder.Object();

        foreach (var field in schema.Fields.ForApi(withHidden))
        {
            var property =
                JsonTypeVisitor.BuildProperty(
                    field, components, schema,
                    factory,
                    withHidden,
                    withComponents);

            // Property is null for UI fields.
            if (property != null)
            {
                var propertyObj = JsonTypeBuilder.ObjectProperty(property);

                // Property is not required because not all languages might be required.
                propertyObj.SetRequired(false);
                propertyObj.SetDescription(field);

                jsonSchema.Properties.Add(field.Name, propertyObj);
            }
        }

        return jsonSchema;
    }

    public static JsonSchema BuildJsonSchema(this Schema schema, PartitionResolver partitionResolver,
        ResolvedComponents components,
        JsonTypeFactory? factory = null,
        bool withHidden = false,
        bool withComponents = false)
    {
        Guard.NotNull(partitionResolver);
        Guard.NotNull(components);

        factory ??= DefaultFactory;

        var jsonSchema = JsonTypeBuilder.Object();

        foreach (var field in schema.Fields.ForApi(withHidden))
        {
            var typeName = $"{schema.TypeName()}{field.Name.ToPascalCase()}PropertyDto";

            // Create a reference to give it a nice name in code generation.
            var (reference, actual) = factory(typeName);

            if (actual != null)
            {
                var partitioning = partitionResolver(field.Partitioning);

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    var property =
                        JsonTypeVisitor.BuildProperty(
                            field, components, schema,
                            factory,
                            withHidden,
                            withComponents);

                    // Property is null for UI fields.
                    if (property != null)
                    {
                        var isOptional = partitioning.IsOptional(partitionKey);

                        var name = partitioning.GetName(partitionKey);

                        // Required if property is required and language/partitioning is not optional.
                        property.SetRequired(field.RawProperties.IsRequired && !isOptional);
                        property.SetDescription(name);

                        actual.Properties.Add(partitionKey, property);
                    }
                }
            }

            var propertyReference =
                JsonTypeBuilder.ReferenceProperty(reference)
                    .SetDescription(field)
                    .SetRequired(field.RawProperties.IsRequired);

            jsonSchema.Properties.Add(field.Name, propertyReference);
        }

        return jsonSchema;
    }

    public static JsonSchemaProperty SetDescription(this JsonSchemaProperty jsonProperty, IField field)
    {
        if (!string.IsNullOrWhiteSpace(field.RawProperties.Hints))
        {
            jsonProperty.Description = $"{field.Name} ({field.RawProperties.Hints})";
        }
        else
        {
            jsonProperty.Description = field.Name;
        }

        return jsonProperty;
    }
}
