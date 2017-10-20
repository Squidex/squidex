// ==========================================================================
//  JsonTypeVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using NJsonSchema;

namespace Squidex.Domain.Apps.Core.Schemas.JsonSchema
{
    public sealed class JsonTypeVisitor : IFieldVisitor<JsonProperty>
    {
        private readonly Func<string, JsonSchema4, JsonSchema4> schemaResolver;

        public JsonTypeVisitor(Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            this.schemaResolver = schemaResolver;
        }

        public JsonProperty Visit(AssetsField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("AssetItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        public JsonProperty Visit(BooleanField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.Boolean;
            });
        }

        public JsonProperty Visit(DateTimeField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.String;
                jsonProperty.Format = JsonFormatStrings.DateTime;
            });
        }

        public JsonProperty Visit(GeolocationField field)
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

        public JsonProperty Visit(JsonField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                jsonProperty.Type = JsonObjectType.Object;
            });
        }

        public JsonProperty Visit(NumberField field)
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

        public JsonProperty Visit(ReferencesField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("ReferenceItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        public JsonProperty Visit(StringField field)
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

        public JsonProperty Visit(TagsField field)
        {
            return CreateProperty(field, jsonProperty =>
            {
                var itemSchema = schemaResolver("TagsItem", new JsonSchema4 { Type = JsonObjectType.String });

                jsonProperty.Type = JsonObjectType.Array;
                jsonProperty.Item = itemSchema;
            });
        }

        private static JsonProperty CreateProperty(Field field, Action<JsonProperty> updater)
        {
            var property = new JsonProperty { IsRequired = field.RawProperties.IsRequired };

            updater(property);

            return property;
        }
    }
}
