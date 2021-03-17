// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal static class Converters
    {
        public static ContentData ToContentData(this IDictionary<string, object> source, IComplexGraphType type)
        {
            var result = new ContentData();

            foreach (var field in type.Fields)
            {
                if (source.TryGetValue(field.Name, out var t) && t is IDictionary<string, object> nested && field.ResolvedType is IComplexGraphType complexType)
                {
                    result[field.SourceName()] = nested.ToFieldData(complexType);
                }
            }

            return result;
        }

        public static ContentFieldData ToFieldData(this IDictionary<string, object> source, IComplexGraphType type)
        {
            var result = new ContentFieldData();

            foreach (var field in type.Fields)
            {
                if (source.TryGetValue(field.Name, out var value))
                {
                    if (value is List<object> list && field.ResolvedType.Flatten() is IComplexGraphType nestedType)
                    {
                        var arr = new JsonArray();

                        foreach (var item in list)
                        {
                            if (item is IDictionary<string, object> nested)
                            {
                                arr.Add(nested.ToNestedData(nestedType));
                            }
                        }

                        result[field.SourceName()] = arr;
                    }
                    else
                    {
                        result[field.SourceName()] = JsonConverter.ParseJson(value);
                    }
                }
            }

            return result;
        }

        public static IJsonValue ToNestedData(this IDictionary<string, object> source, IComplexGraphType type)
        {
            var result = JsonValue.Object();

            foreach (var field in type.Fields)
            {
                if (source.TryGetValue(field.Name, out var value))
                {
                    result[field.SourceName()] = JsonConverter.ParseJson(value);
                }
            }

            return result;
        }
    }
}
