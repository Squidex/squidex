// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using GraphQLParser.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

public sealed class JsonGraphType : JsonNoopGraphType
{
    public override object? Serialize(object? value)
    {
        return value;
    }

    public override object? ParseValue(object? value)
    {
        return ParseJson(value);
    }

    public static JsonValue ParseJson(object? input)
    {
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
                    var json = new JsonArray();

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
                    var json = new JsonObject();

                    if (objectValue.Fields != null)
                    {
                        foreach (var field in objectValue.Fields)
                        {
                            json[field.Name.ToString()] = ParseJson(field.Value);
                        }
                    }

                    return json;
                }

            case IEnumerable<object> list:
                {
                    var json = new JsonArray();

                    foreach (var item in list)
                    {
                        json.Add(ParseJson(item));
                    }

                    return json;
                }

            case IDictionary<string, object> obj:
                {
                    var json = new JsonObject();

                    foreach (var (key, value) in obj)
                    {
                        json[key] = ParseJson(value);
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
