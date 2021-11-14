// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class JsonSchemaExtensions
    {
        public static JsonSchema BuildJsonSchemaForComponent(this Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            // Properties can be required because components itself are not localized.
            var jsonSchema = BuildFlatSchema(schema, schemaResolver, components, withHidden, true);

            jsonSchema.Properties.Add(Component.Discriminator, SchemaBuilder.StringProperty(isRequired: true));

            return jsonSchema;
        }

        public static JsonSchema BuildJsonSchemaFlat(this Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            // Properties can not be required because we do not check all languages.
            var jsonSchema = BuildFlatSchema(schema, schemaResolver, components, withHidden, false);

            return jsonSchema;
        }

        private static JsonSchema BuildFlatSchema(Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components, bool withHidden, bool canBeRequired)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(components, nameof(components));

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var property = JsonTypeVisitor.BuildProperty(field, components, schema, schemaResolver, withHidden);

                // Property is null for UI fields.
                if (property != null)
                {
                    property.SetRequired(field.RawProperties.IsRequired && canBeRequired);
                    property.SetDescription(field);

                    jsonSchema.Properties.Add(field.Name, property);
                }
            }

            return jsonSchema;
        }

        public static JsonSchema BuildJsonSchemaDynamic(this Schema schema, SchemaResolver schemaResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(components, nameof(components));

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var propertyItem = JsonTypeVisitor.BuildProperty(field, components, schema, schemaResolver, withHidden);

                // Property is null for UI fields.
                if (propertyItem != null)
                {
                    var property = SchemaBuilder.ObjectProperty(propertyItem);

                    // Property is not required because not all languages might be required.
                    property.SetRequired(false);
                    property.SetDescription(field);

                    jsonSchema.Properties.Add(field.Name, property);
                }
            }

            return jsonSchema;
        }

        public static JsonSchema BuildJsonSchema(this Schema schema, SchemaResolver schemaResolver, PartitionResolver partitionResolver,
            ResolvedComponents components, bool withHidden = false)
        {
            Guard.NotNull(schemaResolver, nameof(schemaResolver));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));
            Guard.NotNull(components, nameof(components));

            var schemaName = schema.TypeName();

            var jsonSchema = SchemaBuilder.Object();

            foreach (var field in schema.Fields.ForApi(withHidden))
            {
                var propertyObject = SchemaBuilder.Object();

                var partitioning = partitionResolver(field.Partitioning);

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    var propertyItem = JsonTypeVisitor.BuildProperty(field, components, schema, schemaResolver, withHidden);

                    // Property is null for UI fields.
                    if (propertyItem != null)
                    {
                        var isOptional = partitioning.IsOptional(partitionKey);

                        var name = partitioning.GetName(partitionKey);

                        // Required if property is required and language/partitioning is not optional.
                        propertyItem.SetRequired(field.RawProperties.IsRequired && !isOptional);
                        propertyItem.SetDescription(name);

                        propertyObject.Properties.Add(partitionKey, propertyItem);
                    }
                }

                if (propertyObject.Properties.Count > 0)
                {
                    // Create a reference to give it a nice name in code generation.
                    var propertyReference = schemaResolver.Register(propertyObject, $"{schemaName}{field.Name.ToPascalCase()}PropertyDto");

                    var property =
                        SchemaBuilder.ReferenceProperty(propertyReference)
                            .SetDescription(field)
                            .SetRequired(field.RawProperties.IsRequired);

                    jsonSchema.Properties.Add(field.Name, property);
                }
            }

            return jsonSchema;
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
