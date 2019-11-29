// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class FilterConverter : JsonClassConverter<FilterNode<IJsonValue>>
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                yield return typeof(CompareFilter<IJsonValue>);
                yield return typeof(FilterNode<IJsonValue>);
                yield return typeof(LogicalFilter<IJsonValue>);
                yield return typeof(NegateFilter<IJsonValue>);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return SupportedTypes.Contains(objectType);
        }

        protected override FilterNode<IJsonValue> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonException($"Expected StartObject, but got {reader.TokenType}.");
            }

            FilterNode<IJsonValue>? result = null;

            PropertyPath? comparePath = null;

            var compareOperator = (CompareOperator)99;

            IJsonValue? compareValue = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value!.ToString()!;

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading filter.");
                        }

                        if (result != null)
                        {
                            throw new JsonSerializationException($"Unexpected property {propertyName}");
                        }

                        switch (propertyName.ToLowerInvariant())
                        {
                            case "not":
                                var filter = serializer.Deserialize<FilterNode<IJsonValue>>(reader)!;

                                result = new NegateFilter<IJsonValue>(filter);
                                break;
                            case "and":
                                var andFilters = serializer.Deserialize<List<FilterNode<IJsonValue>>>(reader)!;

                                result = new LogicalFilter<IJsonValue>(LogicalFilterType.And, andFilters);
                                break;
                            case "or":
                                var orFilters = serializer.Deserialize<List<FilterNode<IJsonValue>>>(reader)!;

                                result = new LogicalFilter<IJsonValue>(LogicalFilterType.Or, orFilters);
                                break;
                            case "path":
                                comparePath = serializer.Deserialize<PropertyPath>(reader);
                                break;
                            case "op":
                                compareOperator = ReadOperator(reader, serializer);
                                break;
                            case "value":
                                compareValue = serializer.Deserialize<IJsonValue>(reader);
                                break;
                        }

                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        if (result != null)
                        {
                            return result;
                        }

                        if (comparePath == null)
                        {
                            throw new JsonSerializationException("Path not defined.");
                        }

                        if (compareValue == null && compareOperator != CompareOperator.Empty)
                        {
                            throw new JsonSerializationException("Value not defined.");
                        }

                        if (!compareOperator.IsEnumValue())
                        {
                            throw new JsonSerializationException("Operator not defined.");
                        }

                        return new CompareFilter<IJsonValue>(comparePath, compareOperator, compareValue ?? JsonValue.Null);
                }
            }

            throw new JsonSerializationException("Unexpected end when reading filter.");
        }

        private static CompareOperator ReadOperator(JsonReader reader, JsonSerializer serializer)
        {
            var value = serializer.Deserialize<string>(reader)!;

            switch (value.ToLowerInvariant())
            {
                case "eq":
                    return CompareOperator.Equals;
                case "ne":
                    return CompareOperator.NotEquals;
                case "lt":
                    return CompareOperator.LessThan;
                case "le":
                    return CompareOperator.LessThanOrEqual;
                case "gt":
                    return CompareOperator.GreaterThan;
                case "ge":
                    return CompareOperator.GreaterThanOrEqual;
                case "empty":
                    return CompareOperator.Empty;
                case "contains":
                    return CompareOperator.Contains;
                case "endswith":
                    return CompareOperator.EndsWith;
                case "startswith":
                    return CompareOperator.StartsWith;
                case "in":
                    return CompareOperator.In;
            }

            throw new JsonSerializationException($"Unexpected compare operator, got {value}.");
        }

        protected override void WriteValue(JsonWriter writer, FilterNode<IJsonValue> value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
