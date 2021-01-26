// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal sealed class JsonConverter : IAstFromValueConverter
    {
        public static readonly JsonConverter Instance = new JsonConverter();

        private JsonConverter()
        {
        }

        public IValue Convert(object value, IGraphType type)
        {
            return new JsonValueNode(ParseJson(value));
        }

        public bool Matches(object value, IGraphType type)
        {
            return type is JsonGraphType;
        }

        public static IJsonValue ParseJson(object value)
        {
            switch (value)
            {
                case ListValue listValue:
                    return ParseJson(listValue.Value);

                case ObjectValue objectValue:
                    return ParseJson(objectValue.Value);

                case Dictionary<string, object> dictionary:
                    {
                        var json = JsonValue.Object();

                        foreach (var (key, inner) in dictionary)
                        {
                            json[key] = ParseJson(inner);
                        }

                        return json;
                    }

                case List<object> list:
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
    }
}
