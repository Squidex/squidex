// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public delegate JsonSchema SchemaResolver(string name, Func<JsonSchema> schema);

    internal sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty?, JsonTypeVisitor.Args>
    {
        private static readonly JsonTypeVisitor Instance = new JsonTypeVisitor();

        public readonly struct Args
        {
            public readonly SchemaResolver? SchemaResolver;

            public readonly bool WithHiddenFields;

            public Args(SchemaResolver? schemaResolver, bool withHiddenFields)
            {
                SchemaResolver = schemaResolver;

                WithHiddenFields = withHiddenFields;
            }
        }

        private JsonTypeVisitor()
        {
        }

        public static JsonSchemaProperty? BuildProperty(IField field, SchemaResolver? schemaResolver = null, bool withHiddenFields = false)
        {
            var args = new Args(schemaResolver, withHiddenFields);

            return field.Accept(Instance, args);
        }

        public JsonSchemaProperty? Visit(IArrayField field, Args args)
        {
            var itemSchema = SchemaBuilder.Object();

            foreach (var nestedField in field.Fields.ForApi(args.WithHiddenFields))
            {
                var nestedProperty = nestedField.Accept(this, args);

                if (nestedProperty != null)
                {
                    nestedProperty.Description = nestedField.RawProperties.Hints;
                    nestedProperty.SetRequired(nestedField.RawProperties.IsRequired);

                    itemSchema.Properties.Add(nestedField.Name, nestedProperty);
                }
            }

            return SchemaBuilder.ArrayProperty(itemSchema);
        }

        public JsonSchemaProperty? Visit(IField<AssetsFieldProperties> field, Args args)
        {
            return SchemaBuilder.ArrayProperty(SchemaBuilder.String());
        }

        public JsonSchemaProperty? Visit(IField<BooleanFieldProperties> field, Args args)
        {
            return SchemaBuilder.BooleanProperty();
        }

        public JsonSchemaProperty? Visit(IField<ComponentFieldProperties> field, Args args)
        {
            var property = SchemaBuilder.ObjectProperty();

            property.Description = field.RawProperties.Hints;
            property.SetRequired(field.RawProperties.IsRequired);

            property.Properties.Add(Component.Discriminator, SchemaBuilder.StringProperty(isRequired: true));

            if (args.SchemaResolver != null)
            {
                var schemas =
                    field.Properties.SchemaIds?
                        .Select(x => field.GetResolvedSchema(x)).NotNull() ?? Enumerable.Empty<Schema>();

                var discriminator = new OpenApiDiscriminator
                {
                    PropertyName = Component.Discriminator
                };

                foreach (var schema in schemas)
                {
                    var schemaName = $"{schema.TypeName()}ComponentDto";

                    var componentSchema = args.SchemaResolver(schemaName, () =>
                    {
                        var componentSchema = SchemaBuilder.Object();

                        foreach (var sharedField in schema.Fields.ForApi(args.WithHiddenFields))
                        {
                            var sharedProperty = sharedField.Accept(this, args);

                            if (sharedProperty != null)
                            {
                                sharedProperty.Description = sharedField.RawProperties.Hints;
                                sharedProperty.SetRequired(sharedField.RawProperties.IsRequired);

                                componentSchema.Properties.Add(sharedField.Name, sharedProperty);
                            }
                        }

                        componentSchema.Properties.Add(Component.Discriminator, SchemaBuilder.StringProperty(isRequired: true));

                        return componentSchema;
                    });

                    property.OneOf.Add(componentSchema);

                    discriminator.Mapping[schemaName] = componentSchema;
                }

                property.DiscriminatorObject = discriminator;
            }
            else
            {
                property.AllowAdditionalProperties = true;
            }

            return property;
        }

        public JsonSchemaProperty? Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return SchemaBuilder.DateTimeProperty();
        }

        public JsonSchemaProperty? Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            var property = SchemaBuilder.ObjectProperty();

            property.Format = GeoJson.Format;

            return property;
        }

        public JsonSchemaProperty? Visit(IField<JsonFieldProperties> field, Args args)
        {
            return SchemaBuilder.JsonProperty();
        }

        public JsonSchemaProperty? Visit(IField<NumberFieldProperties> field, Args args)
        {
            var property = SchemaBuilder.NumberProperty();

            if (field.Properties.MinValue != null)
            {
                property.Minimum = (decimal)field.Properties.MinValue.Value;
            }

            if (field.Properties.MaxValue != null)
            {
                property.Maximum = (decimal)field.Properties.MaxValue.Value;
            }

            return property;
        }

        public JsonSchemaProperty? Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            return SchemaBuilder.ArrayProperty(SchemaBuilder.String());
        }

        public JsonSchemaProperty? Visit(IField<StringFieldProperties> field, Args args)
        {
            var property = SchemaBuilder.StringProperty();

            property.MaxLength = field.Properties.MaxLength;
            property.MinLength = field.Properties.MinLength;

            property.Pattern = field.Properties.Pattern;

            if (field.Properties.AllowedValues != null)
            {
                var names = property.EnumerationNames ??= new Collection<string>();

                foreach (var value in field.Properties.AllowedValues)
                {
                    names.Add(value);
                }
            }

            return property;
        }

        public JsonSchemaProperty? Visit(IField<TagsFieldProperties> field, Args args)
        {
            return SchemaBuilder.ArrayProperty(SchemaBuilder.String());
        }

        public JsonSchemaProperty? Visit(IField<UIFieldProperties> field, Args args)
        {
            return null;
        }
    }
}
