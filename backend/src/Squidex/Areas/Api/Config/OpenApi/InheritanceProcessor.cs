// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class InheritanceProcessor : ISchemaProcessor
{
    private readonly JsonSerializerOptions options;

    public InheritanceProcessor(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public void Process(SchemaProcessorContext context)
    {
        var typeInfo = options.GetTypeInfo(context.ContextualType.Type);

        if (typeInfo.PolymorphismOptions != null)
        {
            var discriminatorName = typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName;

            var discriminator = new OpenApiDiscriminator
            {
                PropertyName = discriminatorName
            };

            var schema = context.Schema;

            foreach (var derivedType in typeInfo.PolymorphismOptions.DerivedTypes)
            {
                if (derivedType.TypeDiscriminator == null)
                {
                    continue;
                }

                var derivedSchema = context.Generator.Generate(derivedType.DerivedType, context.Resolver);

                discriminator.Mapping[derivedType.TypeDiscriminator.ToString()!] = new JsonSchema
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
