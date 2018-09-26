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
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = new JsonSchema4
                {
                    Type = JsonObjectType.Object
                };

                foreach (var nestedField in field.Fields.Where(x => !x.IsHidden))
                {
                    var childProperty = nestedField.Accept(this);

                    childProperty.Description = nestedField.RawProperties.Hints;
                    childProperty.IsRequired = nestedField.RawProperties.IsRequired;

                    itemSchema.Properties.Add(nestedField.Name, childProperty);
                }

                jsonProperty.Type = JsonObjectType.Object;
                jsonProperty.Item = itemSchema;
            });
        }

        public JsonProperty Visit(IField<AssetsFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("AssetItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        public JsonProperty Visit(IField<BooleanFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.Boolean;
            });
        }

        public JsonProperty Visit(IField<DateTimeFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.String;
                jsonProperty.Format = JsonFormatStrings.DateTime;
            });
        }

        public JsonProperty Visit(IField<GeolocationFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var geolocationSchema = new JsonSchema4
                {
                    AllowAdditionalProperties = false
                };

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

                var schemaReference = schemaResolver("GeolocationDto", geolocationSchema);

                jsonProperty.Type = JsonObjectType.Object;
                jsonProperty.Reference = schemaReference;
            });
        }

        public JsonProperty Visit(IField<JsonFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.Object;
            });
        }

        public JsonProperty Visit(IField<NumberFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.Number;

                if (field.Properties.MinValue.HasValue)
                {
                    jsonProperty.Minimum = (decimal)field.Properties.MinValue.Value;
                }

                if (field.Properties.MaxValue.HasValue)
                {
                    jsonProperty.Maximum = (decimal)field.Properties.MaxValue.Value;
                }
            });
        }

        public JsonProperty Visit(IField<ReferencesFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("ReferenceItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        public JsonProperty Visit(IField<StringFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.String;

                jsonProperty.MinLength = field.Properties.MinLength;
                jsonProperty.MaxLength = field.Properties.MaxLength;

                if (field.Properties.AllowedValues != null)
                {
                    var names = jsonProperty.EnumerationNames = jsonProperty.EnumerationNames ?? new Collection<string>();

                    foreach (var value in field.Properties.AllowedValues)
                    {
                        names.Add(value);
                    }
                }
            });
        }

        public JsonProperty Visit(IField<TagsFieldProperties> field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("TagsItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        private static JsonProperty CreateProperty(IField field, Action<JsonProperty> updater)
        {
            var property = new JsonProperty { IsRequired = field.RawProperties.IsRequired };

            updater(property);

            return property;
        }
    }
}
