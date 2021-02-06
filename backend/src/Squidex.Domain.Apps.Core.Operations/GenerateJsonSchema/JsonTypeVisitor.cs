// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public delegate JsonSchema SchemaResolver(string name, Func<JsonSchema> schema);

    internal sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty?, JsonTypeVisitor.Args>
    {
        private static readonly JsonTypeVisitor Instance = new JsonTypeVisitor();

        public readonly struct Args
        {
            public readonly SchemaResolver SchemaResolver;

            public readonly bool WithHiddenFields;

            public Args(SchemaResolver schemaResolver, bool withHiddenFields)
            {
                SchemaResolver = schemaResolver;

                WithHiddenFields = withHiddenFields;
            }
        }

        private JsonTypeVisitor()
        {
        }

        public static JsonSchemaProperty? BuildProperty(IField field, SchemaResolver schemaResolver, bool withHiddenFields)
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

        public JsonSchemaProperty? Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            return SchemaBuilder.DateTimeProperty();
        }

        public JsonSchemaProperty? Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            var reference = args.SchemaResolver("GeolocationDto", () =>
            {
                var geolocationSchema = SchemaBuilder.Object();

                geolocationSchema.Format = GeoJson.Format;

                geolocationSchema.Properties.Add("latitude", new JsonSchemaProperty
                {
                    Type = JsonObjectType.Number,
                    Maximum = 90,
                    Minimum = -90
                }.SetRequired(true));

                geolocationSchema.Properties.Add("longitude", new JsonSchemaProperty
                {
                    Type = JsonObjectType.Number,
                    Maximum = 180,
                    Minimum = -180
                }.SetRequired(true));

                return geolocationSchema;
            });

            return SchemaBuilder.ObjectProperty(reference);
        }

        public JsonSchemaProperty? Visit(IField<JsonFieldProperties> field, Args args)
        {
            return SchemaBuilder.JsonProperty();
        }

        public JsonSchemaProperty? Visit(IField<NumberFieldProperties> field, Args args)
        {
            var property = SchemaBuilder.NumberProperty();

            if (field.Properties.MinValue.HasValue)
            {
                property.Minimum = (decimal)field.Properties.MinValue.Value;
            }

            if (field.Properties.MaxValue.HasValue)
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
