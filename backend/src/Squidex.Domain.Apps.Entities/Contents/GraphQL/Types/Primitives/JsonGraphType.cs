// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Language.AST;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
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

        public static IJsonValue ParseJson(object? value)
        {
            switch (value)
            {
                case ListValue listValue:
                    return ParseJson(listValue.Value);

                case ObjectValue objectValue:
                    return ParseJson(objectValue.Value);

                case IReadOnlyDictionary<string, object> dictionary:
                    {
                        var json = JsonValue.Object();

                        foreach (var (key, inner) in dictionary)
                        {
                            json[key] = ParseJson(inner);
                        }

                        return json;
                    }

                case IEnumerable<object> list:
                    {
                        var array = JsonValue.Array();

                        foreach (var item in list)
                        {
                            array.Add(ParseJson(item));
                        }

                        return array;
                    }

                default:
                    return JsonValue.Create(value);
            }
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is JsonValueNode jsonGraphType)
            {
                return jsonGraphType.Value;
            }

            return value;
        }

        public override IValue ToAST(object? value)
        {
            return new JsonValueNode(ParseJson(value));
        }
    }
}
