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

    public sealed class JsonTypeVisitor : IFieldVisitor<JsonSchemaProperty>
    {
        private readonly SchemaResolver schemaResolver;
        private readonly bool withHiddenFields;

        public JsonTypeVisitor(SchemaResolver schemaResolver, bool withHiddenFields)
        {
            this.schemaResolver = schemaResolver;

            this.withHiddenFields = withHiddenFields;
        }

        public JsonSchemaProperty Visit(IArrayField field)
        {
            var item = Builder.Object();

            foreach (var nestedField in field.Fields.ForApi(withHiddenFields))
            {
                var childProperty = nestedField.Accept(this);

                if (childProperty != null)
                {
                    childProperty.Description = nestedField.RawProperties.Hints;
                    childProperty.IsRequired = nestedField.RawProperties.IsRequired;

                    item.Properties.Add(nestedField.Name, childProperty);
                }
            }

            return Builder.ArrayProperty(item);
        }

        public JsonSchemaProperty Visit(IField<AssetsFieldProperties> field)
        {
            var item = schemaResolver("AssetItem", Builder.Guid());

            return Builder.ArrayProperty(item);
        }

        public JsonSchemaProperty Visit(IField<BooleanFieldProperties> field)
        {
            return Builder.BooleanProperty();
        }

        public JsonSchemaProperty Visit(IField<DateTimeFieldProperties> field)
        {
            return Builder.DateTimeProperty();
        }

        public JsonSchemaProperty Visit(IField<GeolocationFieldProperties> field)
        {
            var geolocationSchema = Builder.Object();

            geolocationSchema.Properties.Add("latitude", new JsonSchemaProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -90,
                Maximum = 90,
                IsRequired = true
            });

            geolocationSchema.Properties.Add("longitude", new JsonSchemaProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -180,
                Maximum = 180,
                IsRequired = true
            });

            var reference = schemaResolver("GeolocationDto", geolocationSchema);

            return Builder.ObjectProperty(reference);
        }

        public JsonSchemaProperty Visit(IField<JsonFieldProperties> field)
        {
            return Builder.StringProperty();
        }

        public JsonSchemaProperty Visit(IField<NumberFieldProperties> field)
        {
            var property = Builder.NumberProperty();

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

        public JsonSchemaProperty Visit(IField<ReferencesFieldProperties> field)
        {
            var item = schemaResolver("ReferenceItem", Builder.Guid());

            return Builder.ArrayProperty(item);
        }

        public JsonSchemaProperty Visit(IField<StringFieldProperties> field)
        {
            var property = Builder.StringProperty();

            property.MinLength = field.Properties.MinLength;
            property.MaxLength = field.Properties.MaxLength;

            if (field.Properties.AllowedValues != null)
            {
                var names = property.EnumerationNames = property.EnumerationNames ?? new Collection<string>();

                foreach (var value in field.Properties.AllowedValues)
                {
                    names.Add(value);
                }
            }

            return property;
        }

        public JsonSchemaProperty Visit(IField<TagsFieldProperties> field)
        {
            var item = schemaResolver("ReferenceItem", Builder.String());

            return Builder.ArrayProperty(item);
        }

        public JsonSchemaProperty Visit(IField<UIFieldProperties> field)
        {
            return null;
        }
    }
}
