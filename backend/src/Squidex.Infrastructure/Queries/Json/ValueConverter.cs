// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using NodaTime.Text;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class ValueConverter
    {
        private delegate bool Parser<T>(List<string> errors, PropertyPath path, IJsonValue value, out T result);

        private static readonly InstantPattern[] InstantPatterns =
        {
            InstantPattern.General,
            InstantPattern.ExtendedIso,
            InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd")
        };

        public static ClrValue? Convert(FilterField field, IJsonValue value, PropertyPath path, List<string> errors)
        {
            ClrValue? result = null;

            var type = field.Schema.Type;

            if (value is JsonNull && type != FilterSchemaType.GeoObject && field.IsNullable)
            {
                return ClrValue.Null;
            }

            switch (type)
            {
                case FilterSchemaType.GeoObject:
                    {
                        if (TryParseGeoJson(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Any:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            var array = ParseArray<ClrValue?>(errors, path, jsonArray, TryParseDynamic);

                            result = array.Select(x => x?.Value).ToList();
                        }
                        else if (TryParseDynamic(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Boolean:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            result = ParseArray<bool>(errors, path, jsonArray, TryParseBoolean);
                        }
                        else if (TryParseBoolean(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Number:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            result = ParseArray<double>(errors, path, jsonArray, TryParseNumber);
                        }
                        else if (TryParseNumber(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Guid:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            result = ParseArray<Guid>(errors, path, jsonArray, TryParseGuid);
                        }
                        else if (TryParseGuid(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.DateTime:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            result = ParseArray<Instant>(errors, path, jsonArray, TryParseDateTime);
                        }
                        else if (TryParseDateTime(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.StringArray:
                case FilterSchemaType.String:
                    {
                        if (value is JsonArray jsonArray)
                        {
                            result = ParseArray<string>(errors, path, jsonArray, TryParseString!);
                        }
                        else if (TryParseString(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                default:
                    {
                        errors.Add(Errors.WrongType(type.ToString(), path));
                        break;
                    }
            }

            return result;
        }

        private static List<T> ParseArray<T>(List<string> errors, PropertyPath path, JsonArray array, Parser<T> parser)
        {
            var items = new List<T>();

            foreach (var item in array)
            {
                if (parser(errors, path, item, out var temp))
                {
                    items.Add(temp);
                }
            }

            return items;
        }

        private static bool TryParseGeoJson(List<string> errors, PropertyPath path, IJsonValue value, out FilterSphere result)
        {
            const string expected = "Object(geo-json)";

            result = default!;

            if (value is JsonObject geoObject &&
                geoObject.TryGetValue<JsonNumber>("latitude", out var lat) &&
                geoObject.TryGetValue<JsonNumber>("longitude", out var lon) &&
                geoObject.TryGetValue<JsonNumber>("distance", out var distance))
            {
                result = new FilterSphere(lon.Value, lat.Value, distance.Value);

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseBoolean(List<string> errors, PropertyPath path, IJsonValue value, out bool result)
        {
            const string expected = "Boolean";

            result = default;

            if (value is JsonBoolean jsonBoolean)
            {
                result = jsonBoolean.Value;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseNumber(List<string> errors, PropertyPath path, IJsonValue value, out double result)
        {
            const string expected = "Number";

            result = default;

            if (value is JsonNumber jsonNumber)
            {
                result = jsonNumber.Value;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseString(List<string> errors, PropertyPath path, IJsonValue value, out string? result)
        {
            const string expected = "String";

            result = default;

            if (value is JsonString jsonString)
            {
                result = jsonString.Value;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseGuid(List<string> errors, PropertyPath path, IJsonValue value, out Guid result)
        {
            const string expected = "String (Guid)";

            result = default;

            if (value is JsonString jsonString)
            {
                if (Guid.TryParse(jsonString.Value, out result))
                {
                    return true;
                }

                errors.Add(Errors.WrongFormat(expected, path));
            }
            else
            {
                errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));
            }

            return false;
        }

        private static bool TryParseDateTime(List<string> errors, PropertyPath path, IJsonValue value, out Instant result)
        {
            const string expected = "String (ISO8601 DateTime)";

            result = default;

            if (value is JsonString jsonString)
            {
                foreach (var pattern in InstantPatterns)
                {
                    var parsed = pattern.Parse(jsonString.Value);

                    if (parsed.Success)
                    {
                        result = parsed.Value;

                        return true;
                    }
                }

                errors.Add(Errors.WrongFormat(expected, path));
            }
            else
            {
                errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));
            }

            return false;
        }

        private static bool TryParseDynamic(List<string> errors, PropertyPath path, IJsonValue value, out ClrValue? result)
        {
            result = null;

            switch (value)
            {
                case JsonNull:
                    return true;
                case JsonNumber jsonNumber:
                    result = jsonNumber.Value;
                    return true;
                case JsonBoolean jsonBoolean:
                    result = jsonBoolean.Value;
                    return true;
                case JsonString jsonString:
                    {
                        if (Guid.TryParse(jsonString.Value, out var guid))
                        {
                            result = guid;

                            return true;
                        }

                        foreach (var pattern in InstantPatterns)
                        {
                            var parsed = pattern.Parse(jsonString.Value);

                            if (parsed.Success)
                            {
                                result = parsed.Value;

                                return true;
                            }
                        }

                        result = jsonString.Value;

                        return true;
                    }
            }

            errors.Add(Errors.WrongPrimitive(value.Type.ToString(), path));

            return false;
        }
    }
}
