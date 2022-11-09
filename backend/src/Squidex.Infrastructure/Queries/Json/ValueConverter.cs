// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using NodaTime.Text;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Queries.Json;

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

        if (value == default && type != FilterSchemaType.GeoObject && field.IsNullable)
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
                    if (value.Value is JsonArray a)
                    {
                        var array = ParseArray<ClrValue?>(errors, path, a, TryParseDynamic);

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
                    if (value.Value is JsonArray a)
                    {
                        result = ParseArray<bool>(errors, path, a, TryParseBoolean);
                    }
                    else if (TryParseBoolean(errors, path, value, out var temp))
                    {
                        result = temp;
                    }

                    break;
                }

            case FilterSchemaType.Number:
                {
                    if (value.Value is JsonArray a)
                    {
                        result = ParseArray<double>(errors, path, a, TryParseNumber);
                    }
                    else if (TryParseNumber(errors, path, value, out var temp))
                    {
                        result = temp;
                    }

                    break;
                }

            case FilterSchemaType.Guid:
                {
                    if (value.Value is JsonArray a)
                    {
                        result = ParseArray<Guid>(errors, path, a, TryParseGuid);
                    }
                    else if (TryParseGuid(errors, path, value, out var temp))
                    {
                        result = temp;
                    }

                    break;
                }

            case FilterSchemaType.DateTime:
                {
                    if (value.Value is JsonArray a)
                    {
                        result = ParseArray<Instant>(errors, path, a, TryParseDateTime);
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
                    if (value.Value is JsonArray a)
                    {
                        result = ParseArray<string>(errors, path, a, TryParseString!);
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

        if (value.Value is JsonObject o &&
            o.TryGetValue("latitude", out var found) && found.Value is double lat &&
            o.TryGetValue("longitude", out found) && found.Value is double lon &&
            o.TryGetValue("distance", out found) && found.Value is double distance)
        {
            result = new FilterSphere(lon, lat, distance);

            return true;
        }

        errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

        return false;
    }

    private static bool TryParseBoolean(List<string> errors, PropertyPath path, JsonValue value, out bool result)
    {
        const string expected = "Boolean";

        result = default;

        if (value.Value is bool b)
        {
            result = b;

            return true;
        }

        errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

        return false;
    }

    private static bool TryParseNumber(List<string> errors, PropertyPath path, JsonValue value, out double result)
    {
        const string expected = "Number";

        result = default;

        if (value.Value is double n)
        {
            result = n;

            return true;
        }

        errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

        return false;
    }

    private static bool TryParseString(List<string> errors, PropertyPath path, JsonValue value, out string? result)
    {
        const string expected = "String";

        result = default;

        if (value.Value is string s)
        {
            result = s;

            return true;
        }

        errors.Add(Errors.WrongExpectedType(expected, value.Type.ToString(), path));

        return false;
    }

    private static bool TryParseGuid(List<string> errors, PropertyPath path, JsonValue value, out Guid result)
    {
        const string expected = "String (Guid)";

        result = default;

        if (value.Value is string s)
        {
            if (Guid.TryParse(s, out result))
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

        if (value.Value is string s)
        {
            foreach (var pattern in InstantPatterns)
            {
                var parsed = pattern.Parse(s);

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

        switch (value.Value)
        {
            case null:
                return true;
            case bool b:
                result = b;
                return true;
            case double n:
                result = n;
                return true;
            case string s:
                {
                    if (Guid.TryParse(s, out var guid))
                    {
                        result = guid;

                        return true;
                    }

                    foreach (var pattern in InstantPatterns)
                    {
                        var parsed = pattern.Parse(s);

                        if (parsed.Success)
                        {
                            result = parsed.Value;

                            return true;
                        }
                    }

                    result = s;

                    return true;
                }
        }

        errors.Add(Errors.WrongPrimitive(value.Type.ToString(), path));

        return false;
    }
}
