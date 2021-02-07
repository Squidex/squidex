// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
        public static JsonSchema BuildFlatJsonSchema(this Schema schema, SchemaResolver schemaResolver, bool withHidden = false)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));

            var schemaName = schema.TypeName();

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var property = JsonTypeVisitor.BuildProperty(field, schemaResolver, withHidden);

                if (property != null)
                {
                    var propertyReference = schemaResolver($"{schemaName}{field.Name.ToPascalCase()}FlatPropertyDto", () => property);

                    jsonSchema.Properties.Add(field.Name, CreateProperty(field, propertyReference));
                }
            }

            return jsonSchema;
        }

        public static JsonSchema BuildJsonSchema(this Schema schema, PartitionResolver partitionResolver, SchemaResolver schemaResolver, bool withHidden = false)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            var schemaName = schema.TypeName();

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var partitionObject = SchemaBuilder.Object();
                var partitioning = partitionResolver(field.Partitioning);

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    var partitionItemProperty = JsonTypeVisitor.BuildProperty(field, schemaResolver, withHidden);

                    if (partitionItemProperty != null)
                    {
                        var isOptional = partitioning.IsOptional(partitionKey);

                        var name = partitioning.GetName(partitionKey);

                        partitionItemProperty.Description = name;
                        partitionItemProperty.SetRequired(field.RawProperties.IsRequired && !isOptional);

                        partitionObject.Properties.Add(partitionKey, partitionItemProperty);
                    }
                }

                if (partitionObject.Properties.Count > 0)
                {
                    var propertyReference = schemaResolver($"{schemaName}{field.Name.ToPascalCase()}PropertyDto", () => partitionObject);

                    jsonSchema.Properties.Add(field.Name, CreateProperty(field, propertyReference));
                }
            }

            return jsonSchema;
        }

        public static JsonSchemaProperty CreateProperty(IField field, JsonSchema reference)
        {
            var jsonProperty = SchemaBuilder.ObjectProperty(reference);

            if (!string.IsNullOrWhiteSpace(field.RawProperties.Hints))
            {
                jsonProperty.Description = $"{field.Name} ({field.RawProperties.Hints})";
            }
            else
            {
                jsonProperty.Description = field.Name;
            }

            jsonProperty.SetRequired(field.RawProperties.IsRequired);

            return jsonProperty;
        }
    }
}
