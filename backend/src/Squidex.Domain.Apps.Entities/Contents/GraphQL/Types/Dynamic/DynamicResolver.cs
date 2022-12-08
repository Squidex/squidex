// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Dynamic;

internal sealed class DynamicResolver : IFieldResolver
{
    public static readonly DynamicResolver Instance = new DynamicResolver();

    public async ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        if (context.Source is JsonObject jsonObject)
        {
            var name = context.FieldDefinition.Name;

            if (!jsonObject.TryGetValue(name, out var jsonValue))
            {
                return null;
            }

            var value = Convert(jsonValue);

            return value;
        }

        var result = await NameFieldResolver.Instance.ResolveAsync(context);

        return result;
    }

    private static object? Convert(JsonValue json)
    {
        var value = json.Value;

        switch (value)
        {
            case double d:
                {
                    var asInteger = (long)d;

                    if (asInteger == d)
                    {
                        return asInteger;
                    }

                    break;
                }

            case JsonArray a:
                {
                    var result = new List<object?>();

                    foreach (var item in a)
                    {
                        result.Add(Convert(item));
                    }

                    return result;
                }
        }

        return value;
    }
}
