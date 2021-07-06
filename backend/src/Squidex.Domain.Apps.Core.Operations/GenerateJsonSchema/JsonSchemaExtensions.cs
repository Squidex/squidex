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

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class JsonSchemaExtensions
    {
        public static JsonSchema BuildFlatJsonSchema(this Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var schemaName = schema.TypeName();

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi())
            {
                var property = JsonTypeVisitor.BuildProperty(field, components);

                if (property != null)
                {
                    var propertyReference = schemaResolver($"{schemaName}{field.Name.ToPascalCase()}FlatPropertyDto", () => property);

                    jsonSchema.Properties.Add(field.Name, CreateProperty(field, propertyReference));
                }
            }

            return jsonSchema;
        }

        public static JsonSchema BuildDynamicJsonSchema(this Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var propertyItem = JsonTypeVisitor.BuildProperty(field, components, schemaResolver, withHidden);

                if (propertyItem != null)
                {
                    var property =
                        SchemaBuilder.ObjectProperty(propertyItem)
                            .SetDescription(field)
                            .SetRequired(field.RawProperties.IsRequired);

                    jsonSchema.Properties.Add(field.Name, property);
                }
            }

            return jsonSchema;
        }

        public static JsonSchema BuildJsonSchema(this Schema schema, PartitionResolver partitionResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var propertyObject = SchemaBuilder.Object();

                var partitioning = partitionResolver(field.Partitioning);

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    var propertyItem = JsonTypeVisitor.BuildProperty(field, components, withHiddenFields: withHidden);

                    if (propertyItem != null)
                    {
                        var isOptional = partitioning.IsOptional(partitionKey);

                        var name = partitioning.GetName(partitionKey);

                        propertyItem.SetDescription(name);
                        propertyItem.SetRequired(field.RawProperties.IsRequired && !isOptional);

                        propertyObject.Properties.Add(partitionKey, propertyItem);
                    }
                }

                if (propertyObject.Properties.Count > 0)
                {
                    jsonSchema.Properties.Add(field.Name, CreateProperty(field, propertyObject));
                }
            }

            return jsonSchema;
        }

        public static JsonSchemaProperty CreateProperty(IField field, JsonSchema reference)
        {
            var jsonProperty =
                SchemaBuilder.ReferenceProperty(reference)
                    .SetDescription(field)
                    .SetRequired(field.RawProperties.IsRequired);

            return jsonProperty;
        }

        private static JsonSchemaProperty SetDescription(this JsonSchemaProperty jsonProperty, IField field)
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
}
