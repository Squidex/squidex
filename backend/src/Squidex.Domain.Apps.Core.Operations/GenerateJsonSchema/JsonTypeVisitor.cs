// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public delegate JsonSchema SchemaResolver(string name, JsonSchema schema);

    public sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty?>
    {
        private readonly SchemaResolver schemaResolver;
        private readonly bool withHiddenFields;

        public JsonTypeVisitor(SchemaResolver schemaResolver, bool withHiddenFields)
        {
            this.schemaResolver = schemaResolver;

            this.withHiddenFields = withHiddenFields;
        }

        public JsonSchemaProperty? Visit(IArrayField field)
        {
            var itemSchema = SchemaBuilder.Object();

            foreach (var nestedField in field.Fields.ForApi(withHiddenFields))
            {
                var nestedProperty = nestedField.Accept(this);

                if (nestedProperty != null)
                {
                    nestedProperty.Description = nestedField.RawProperties.Hints;
                    nestedProperty.SetRequired(nestedField.RawProperties.IsRequired);

                    itemSchema.Properties.Add(nestedField.Name, nestedProperty);
                }
            }

            return SchemaBuilder.ArrayProperty(itemSchema);
        }

        public JsonSchemaProperty? Visit(IField<AssetsFieldProperties> field)
        {
            var itemSchema = schemaResolver("AssetItem", SchemaBuilder.Guid());

            return SchemaBuilder.ArrayProperty(itemSchema);
        }

        public JsonSchemaProperty? Visit(IField<BooleanFieldProperties> field)
        {
            return SchemaBuilder.BooleanProperty();
        }

        public JsonSchemaProperty? Visit(IField<DateTimeFieldProperties> field)
        {
            return SchemaBuilder.DateTimeProperty();
        }

        public JsonSchemaProperty? Visit(IField<GeolocationFieldProperties> field)
        {
            var geolocationSchema = SchemaBuilder.Object();

            geolocationSchema.Properties.Add("latitude", new JsonSchemaProperty
            {
                Type = JsonObjectType.Number,
                IsNullableRaw = false,
                IsRequired = true,
                Maximum = 90,
                Minimum = -90
            });

            geolocationSchema.Properties.Add("longitude", new JsonSchemaProperty
            {
                Type = JsonObjectType.Number,
                IsNullableRaw = false,
                IsRequired = true,
                Maximum = 180,
                Minimum = -180
            });

            var reference = schemaResolver("GeolocationDto", geolocationSchema);

            return SchemaBuilder.ObjectProperty(reference);
        }

        public JsonSchemaProperty? Visit(IField<JsonFieldProperties> field)
        {
            return SchemaBuilder.JsonProperty();
        }

        public JsonSchemaProperty? Visit(IField<NumberFieldProperties> field)
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

        public JsonSchemaProperty? Visit(IField<ReferencesFieldProperties> field)
        {
            var itemSchema = schemaResolver("ReferenceItem", SchemaBuilder.Guid());

            return SchemaBuilder.ArrayProperty(itemSchema);
        }

        public JsonSchemaProperty? Visit(IField<StringFieldProperties> field)
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

        public JsonSchemaProperty? Visit(IField<TagsFieldProperties> field)
        {
            var itemSchema = schemaResolver("ReferenceItem", SchemaBuilder.String());

            return SchemaBuilder.ArrayProperty(itemSchema);
        }

        public JsonSchemaProperty? Visit(IField<UIFieldProperties> field)
        {
            return null;
        }
    }
}
