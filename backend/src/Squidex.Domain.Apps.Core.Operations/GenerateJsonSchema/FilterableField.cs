// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

#pragma warning disable CA1822 // Mark members as static

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public sealed record FilterableField
    {
        public FilterableFieldType Type { get; init; }

        public string? FieldDescription { get; init; }

        public string FieldPath { get; init; }

        public bool IsNullable { get; init; }

        public object? Extra { get; init; }
    }

    public sealed class FilterableFieldModel
    {
        private static readonly Dictionary<FilterableFieldType, IReadOnlyList<CompareOperator>> AllOperators = new Dictionary<FilterableFieldType, IReadOnlyList<CompareOperator>>
        {
        };

        public IReadOnlyList<FilterableField> Fields { get; init; }

        public IReadOnlyDictionary<FilterableFieldType, IReadOnlyList<CompareOperator>> Operators => AllOperators;

        public static FilterableFieldModel Build(JsonSchema schema)
        {
            var prefixes = new Stack<string>();

            var fields = new List<FilterableField>();

            void AddField(FilterableFieldType type, JsonSchema schema, object? extra = null)
            {
                if (prefixes.Count == 0)
                {
                    return;
                }

                var path = string.Join('.', prefixes.Reverse());

                fields.Add(new FilterableField
                {
                    Type = type,
                    FieldDescription = schema.Description,
                    FieldPath = path,
                    Extra = extra
                });
            }

            static bool TryGetSchemaIds(JsonSchema schema, out object? schemaIds)
            {
                schemaIds = null;

                return schema.ExtensionData?.TryGetValue("schemaId", out schemaIds) == true;
            }

            void CheckField(JsonSchema schema)
            {
                switch (schema.Type)
                {
                    case JsonObjectType.Boolean:
                        AddField(FilterableFieldType.Boolean, schema);
                        break;
                    case JsonObjectType.Number:
                        AddField(FilterableFieldType.Number, schema);
                        break;
                    case JsonObjectType.String when schema.Format == JsonFormatStrings.DateTime:
                        AddField(FilterableFieldType.DateTime, schema);
                        break;
                    case JsonObjectType.String when schema.Format == SpecialFormats.Status:
                        AddField(FilterableFieldType.Status, schema);
                        break;
                    case JsonObjectType.String when schema.Format == SpecialFormats.User:
                        AddField(FilterableFieldType.User, schema);
                        break;
                    case JsonObjectType.String:
                        AddField(FilterableFieldType.String, schema);
                        break;
                    case JsonObjectType.Array when TryGetSchemaIds(schema, out var schemaIds):
                        AddField(FilterableFieldType.String, schema, schemaIds);
                        break;
                    case JsonObjectType.Array when schema.Item != null:
                        CheckField(schema.Item);
                        break;
                    case JsonObjectType.Object when schema.Format == GeoJson.Format:
                        AddField(FilterableFieldType.GeoObject, schema);
                        break;
                    case JsonObjectType.Object when schema.Reference != null:
                        CheckField(schema.Reference);
                        break;
                    case JsonObjectType.None when schema.Reference != null:
                        CheckField(schema.Reference);
                        break;
                    case JsonObjectType.Object:
                        {
                            foreach (var (name, property) in schema.Properties)
                            {
                                prefixes.Push(name);
                                CheckField(property);
                                prefixes.Pop();
                            }

                            if (schema.DiscriminatorObject != null)
                            {
                                foreach (var mapping in schema.DiscriminatorObject.Mapping.Values)
                                {
                                    CheckField(mapping);
                                }
                            }

                            break;
                        }
                }
            }

            CheckField(schema);

            return new FilterableFieldModel { Fields = fields };
        }
    }

    public enum FilterableFieldType
    {
        Boolean,
        Date,
        DateTime,
        GeoObject,
        Number,
        Reference,
        Status,
        String,
        Tags,
        User
    }
}
