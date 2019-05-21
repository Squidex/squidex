// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class JsonSchemaExtensions
    {
        public static JsonSchema4 BuildJsonSchema(this Schema schema, PartitionResolver partitionResolver, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = schema.Name.ToPascalCase();

            var jsonTypeVisitor = new JsonTypeVisitor(schemaResolver);
            var jsonSchema = new JsonSchema4 { Type = JsonObjectType.Object };

            foreach (var field in schema.Fields.ForApi())
            {
                var partitionObject = Builder.Object();
                var partition = partitionResolver(field.Partitioning);

                foreach (var partitionItem in partition)
                {
                    var partitionItemProperty = field.Accept(jsonTypeVisitor);

                    if (partitionItemProperty != null)
                    {
                        partitionItemProperty.Description = partitionItem.Name;
                        partitionItemProperty.IsRequired = field.RawProperties.IsRequired && !partitionItem.IsOptional;

                        partitionObject.Properties.Add(partitionItem.Key, partitionItemProperty);
                    }
                }

                if (partitionObject.Properties.Count > 0)
                {
                    var propertyReference = schemaResolver($"{schemaName}{field.Name.ToPascalCase()}Property", partitionObject);

                    jsonSchema.Properties.Add(field.Name, CreateProperty(field, propertyReference));
                }
            }

            return jsonSchema;
        }

        public static JsonProperty CreateProperty(IField field, JsonSchema4 reference)
        {
            var jsonProperty = Builder.ObjectProperty(reference);

            if (!string.IsNullOrWhiteSpace(field.RawProperties.Hints))
            {
                jsonProperty.Description = $"{field.Name} ({field.RawProperties.Hints})";
            }
            else
            {
                jsonProperty.Description = field.Name;
            }

            jsonProperty.IsRequired = field.RawProperties.IsRequired;

            return jsonProperty;
        }
    }
}
