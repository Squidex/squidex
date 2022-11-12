// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NJsonSchema.Generation;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class DiscriminatorProcessor : ISchemaProcessor
{
    private readonly TypeRegistry typeRegistry;

    public DiscriminatorProcessor(TypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;
    }

    public void Process(SchemaProcessorContext context)
    {
        if (context.Schema.DiscriminatorObject != null)
        {
            return;
        }

        if (!typeRegistry.TryGetConfig(context.ContextualType.Type, out var config) ||
            config.DerivedTypes.Count <= 0 ||
            config.DiscriminatorProperty == null)
        {
            return;
        }

        var discriminatorName = config.DiscriminatorProperty;
        var discriminatorObject = new OpenApiDiscriminator
        {
            PropertyName = discriminatorName
        };

        var schema = context.Schema;

        foreach (var (derivedType, typeName) in config.DerivedTypes)
        {
            var derivedSchema = context.Generator.Generate(derivedType, context.Resolver);

            discriminatorObject.Mapping[typeName] = new JsonSchema
            {
                Reference = derivedSchema
            };
        }

        schema.DiscriminatorObject = discriminatorObject;

        if (!schema.Properties.TryGetValue(discriminatorName, out var existingProperty))
        {
            schema.Properties[discriminatorName] = existingProperty = new JsonSchemaProperty
            {
                Type = JsonObjectType.String
            };
        }

        existingProperty.IsRequired = true;
    }
}
