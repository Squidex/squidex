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
        private delegate bool Parser<T>(List<string> errors, PropertyPath path, JsonValue value, out T result);

        private static readonly InstantPattern[] InstantPatterns =
        {
            InstantPattern.General,
            InstantPattern.ExtendedIso,
            InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd")
        };

        public static ClrValue? Convert(FilterField field, JsonValue value, PropertyPath path, List<string> errors)
        {
            ClrValue? result = null;

            var type = field.Schema.Type;

            if (value.Type == JsonValueType.Null && type != FilterSchemaType.GeoObject && field.IsNullable)
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
                        if (value.Type == JsonValueType.Array)
                        {
                            var array = ParseArray<ClrValue?>(errors, path, value.AsArray, TryParseDynamic);

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
                        if (value.Type == JsonValueType.Array)
                        {
                            result = ParseArray<bool>(errors, path, value.AsArray, TryParseBoolean);
                        }
                        else if (TryParseBoolean(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Number:
                    {
                        if (value.Type == JsonValueType.Array)
                        {
                            result = ParseArray<double>(errors, path, value.AsArray, TryParseNumber);
                        }
                        else if (TryParseNumber(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.Guid:
                    {
                        if (value.Type == JsonValueType.Array)
                        {
                            result = ParseArray<Guid>(errors, path, value.AsArray, TryParseGuid);
                        }
                        else if (TryParseGuid(errors, path, value, out var temp))
                        {
                            result = temp;
                        }

                        break;
                    }

                case FilterSchemaType.DateTime:
                    {
                        if (value.Type == JsonValueType.Array)
                        {
                            result = ParseArray<Instant>(errors, path, value.AsArray, TryParseDateTime);
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
                        if (value.Type == JsonValueType.Array)
                        {
                            result = ParseArray<string>(errors, path, value.AsArray, TryParseString!);
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

        private static bool TryParseGeoJson(List<string> errors, PropertyPath path, JsonValue value, out FilterSphere result)
        {
            const string expected = "Object(geo-json)";

            result = default!;

            if (value.Type == JsonValueType.Object &&
                value.TryGetValue("latitude", out var lat) && lat.Type == JsonValueType.Number &&
                value.TryGetValue("longitude", out var lon) && lon.Type == JsonValueType.Number &&
                value.TryGetValue("distance", out var distance) && distance.Type == JsonValueType.Number)
            {
                result = new FilterSphere(lon.AsNumber, lat.AsNumber, distance.AsNumber);

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseBoolean(List<string> errors, PropertyPath path, JsonValue value, out bool result)
        {
            const string expected = "Boolean";

            result = default;

            if (value.Type == JsonValueType.Boolean)
            {
                result = value.AsBoolean;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseNumber(List<string> errors, PropertyPath path, JsonValue value, out double result)
        {
            const string expected = "Number";

            result = default;

            if (value.Type == JsonValueType.Number)
            {
                result = value.AsNumber;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseString(List<string> errors, PropertyPath path, JsonValue value, out string? result)
        {
            const string expected = "String";

            result = default;

            if (value.Type == JsonValueType.String)
            {
                result = value.AsString;

                return true;
            }

            errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

            return false;
        }

        private static bool TryParseGuid(List<string> errors, PropertyPath path, JsonValue value, out Guid result)
        {
            const string expected = "String (Guid)";

            result = default;

            if (value.Type == JsonValueType.String)
            {
                if (Guid.TryParse(value.AsString, out result))
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

        private static bool TryParseDateTime(List<string> errors, PropertyPath path, JsonValue value, out Instant result)
        {
            const string expected = "String (ISO8601 DateTime)";

            result = default;

            if (value.Type == JsonValueType.String)
            {
                var typed = value.AsString;

                foreach (var pattern in InstantPatterns)
                {
                    var parsed = pattern.Parse(typed);

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

        private static bool TryParseDynamic(List<string> errors, PropertyPath path, JsonValue value, out ClrValue? result)
        {
            result = null;

            switch (value.Type)
            {
                case JsonValueType.Null:
                    return true;
                case JsonValueType.Number:
                    result = value.AsNumber;
                    return true;
                case JsonValueType.Boolean:
                    result = value.AsBoolean;
                    return true;
                case JsonValueType.String:
                    {
                        var typed = value.AsString;

                        if (Guid.TryParse(typed, out var guid))
                        {
                            result = guid;

                            return true;
                        }

                        foreach (var pattern in InstantPatterns)
                        {
                            var parsed = pattern.Parse(typed);

                            if (parsed.Success)
                            {
                                result = parsed.Value;

                                return true;
                            }
                        }

                        result = typed;

                        return true;
                    }
            }

            errors.Add(Errors.WrongPrimitive(value.Type.ToString(), path));

            return false;
        }
    }
}
