﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Utilities;
using NJsonSchema;
using NJsonSchema.Generation;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class DiscriminatorProcessor(TypeRegistry typeRegistry) : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.Schema.DiscriminatorObject != null)
        {
            return;
        }

        if (!typeRegistry.TryGetConfig(context.ContextualType.Type, out var config) ||
            config.IsEmpty ||
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

        foreach (var (derivedType, typeName) in config.DerivedTypes().OrderBy(x => x.TypeName))
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
