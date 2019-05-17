// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public sealed class JsonTypeVisitor : IFieldVisitor<JsonProperty>
    {
        private readonly Func<string, JsonSchema4, JsonSchema4> schemaResolver;

        public JsonTypeVisitor(Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            this.schemaResolver = schemaResolver;
        }

        public JsonProperty Visit(IArrayField field)
        {
            var item = Builder.Object();

            foreach (var nestedField in field.Fields.Where(x => !x.IsHidden))
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

        public JsonProperty Visit(IField<AssetsFieldProperties> field)
        {
            var item = schemaResolver("AssetItem", Builder.Guid());

            return Builder.ArrayProperty(item);
        }

        public JsonProperty Visit(IField<BooleanFieldProperties> field)
        {
            return Builder.BooleanProperty();
        }

        public JsonProperty Visit(IField<DateTimeFieldProperties> field)
        {
            return Builder.DateTimeProperty();
        }

        public JsonProperty Visit(IField<GeolocationFieldProperties> field)
        {
            var geolocationSchema = Builder.Object();

            geolocationSchema.Properties.Add("latitude", new JsonProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -90,
                Maximum = 90,
                IsRequired = true
            });

            geolocationSchema.Properties.Add("longitude", new JsonProperty
            {
                Type = JsonObjectType.Number,
                Minimum = -180,
                Maximum = 180,
                IsRequired = true
            });

            var reference = schemaResolver("GeolocationDto", geolocationSchema);

            return Builder.ObjectProperty(reference);
        }

        public JsonProperty Visit(IField<JsonFieldProperties> field)
        {
            return Builder.StringProperty();
        }

        public JsonProperty Visit(IField<NumberFieldProperties> field)
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

        public JsonProperty Visit(IField<ReferencesFieldProperties> field)
        {
            var item = schemaResolver("ReferenceItem", Builder.Guid());

            return Builder.ArrayProperty(item);
        }

        public JsonProperty Visit(IField<StringFieldProperties> field)
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

        public JsonProperty Visit(IField<TagsFieldProperties> field)
        {
            var item = schemaResolver("ReferenceItem", Builder.String());

            return Builder.ArrayProperty(item);
        }

        public JsonProperty Visit(IField<UIFieldProperties> field)
        {
            return null;
        }
    }
}
