// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using GraphQL.Types;
using GraphQLParser.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

public sealed class JsonGraphType : ScalarGraphType
{
    public JsonGraphType()
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "JsonScalar";

        Description = "Unstructured Json object";
    }

    public override object? Serialize(object? value)
    {
        return value;
    }

    public override object? ParseValue(object? value)
    {
        return ParseJson(value);
    }

    public static JsonValue ParseJson(object? input, Func<object, IReadOnlyDictionary<string, string>?>? keyMap = null)
    {
        keyMap ??= x => null;

        switch (input)
        {
            case GraphQLBooleanValue booleanValue:
                return booleanValue.BoolValue;

            case GraphQLFloatValue floatValue:
                return double.Parse((string)floatValue.Value, NumberStyles.Any, CultureInfo.InvariantCulture);

            case GraphQLIntValue intValue:
                return double.Parse((string)intValue.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);

            case GraphQLNullValue:
                return default;

            case GraphQLStringValue stringValue:
                return (string)stringValue.Value;

            case GraphQLListValue listValue:
                {
                    var json = new JsonArray(listValue.Values?.Count ?? 0);

                    if (listValue.Values != null)
                    {
                        foreach (var item in listValue.Values)
                        {
                            json.Add(ParseJson(item));
                        }
                    }

                    return json;
                }

            case GraphQLObjectValue objectValue:
                {
                    var json = new JsonObject(objectValue.Fields?.Count ?? 0);

                    if (objectValue.Fields != null)
                    {
                        var map = keyMap(objectValue);

                        foreach (var field in objectValue.Fields)
                        {
                            var sourceField = field.Name.ToString();

                            if (map?.TryGetValue(sourceField, out var temp) == true)
                            {
                                sourceField = temp;
                            }

                            json[sourceField] = ParseJson(field.Value);
                        }
                    }

                    return json;
                }

            case IEnumerable<object> list:
                {
                    var json = new JsonArray(list.Count());

                    foreach (var item in list)
                    {
                        json.Add(ParseJson(item));
                    }

                    return json;
                }

            case IDictionary<string, object> obj:
                {
                    var json = new JsonObject(obj.Count);

                    if (obj.Count > 0)
                    {
                        var map = keyMap(obj);

                        foreach (var (key, value) in obj)
                        {
                            var sourceField = key;

                            if (map?.TryGetValue(sourceField, out var temp) == true)
                            {
                                sourceField = temp;
                            }

                            json[sourceField] = ParseJson(value);
                        }
                    }

                    return json;
                }

            default:
                return JsonValue.Create(input);
        }
    }

    public override object ParseLiteral(GraphQLValue value)
    {
        if (value is JsonValueNode jsonGraphType)
        {
            return jsonGraphType.Value;
        }

        return value;
    }

    public override GraphQLValue ToAST(object? value)
    {
        return new JsonValueNode(ParseJson(value));
    }
}
