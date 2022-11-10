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
        var config = typeRegistry[context.ContextualType.Type];

        if (config.DerivedTypes.Count > 0 && config.DiscriminatorProperty != null)
        {
            var discriminatorName = config.DiscriminatorProperty;

            var discriminator = new OpenApiDiscriminator
            {
                PropertyName = discriminatorName
            };

            var schema = context.Schema;

            foreach (var (derivedType, typeName) in config.DerivedTypes)
            {
                var derivedSchema = context.Generator.Generate(derivedType, context.Resolver);

                discriminator.Mapping[typeName] = new JsonSchema
                {
                    Reference = derivedSchema
                };
            }

            schema.DiscriminatorObject = discriminator;

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
}
