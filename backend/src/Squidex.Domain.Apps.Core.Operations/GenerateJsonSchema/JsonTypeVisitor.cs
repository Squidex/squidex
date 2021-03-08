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

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public delegate JsonSchema SchemaResolver(string name, Func<JsonSchema> schema);

    internal sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty?, JsonTypeVisitor.Args>
    {
        private static readonly JsonTypeVisitor Instance = new JsonTypeVisitor();

        public readonly struct Args
        {
            public readonly bool WithHiddenFields;

            public Args(bool withHiddenFields)
            {
                WithHiddenFields = withHiddenFields;
            }
        }

        private JsonTypeVisitor()
        {
        }

        public static JsonSchemaProperty? BuildProperty(IField field, bool withHiddenFields)
        {
            var args = new Args(withHiddenFields);

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
                    nestedProperty.SetDescription(nestedField.RawProperties.Hints);
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
            return SchemaBuilder.ObjectProperty();
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

            if (field.Properties.MinLength != null)
            {
                property.Minimum = field.Properties.MinLength.Value;
            }

            if (field.Properties.MaxLength != null)
            {
                property.Maximum = field.Properties.MaxLength.Value;
            }

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
