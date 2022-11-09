// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Dynamic;

internal static class DynamicSchemaBuilder
{
    public static IGraphType[] ParseTypes(string? typeDefinitions, ReservedNames typeNames)
    {
        if (string.IsNullOrWhiteSpace(typeDefinitions))
        {
            return Array.Empty<IGraphType>();
        }

        Schema schema;
        try
        {
            schema = Schema.For(typeDefinitions);
        }
        catch
        {
            return Array.Empty<IGraphType>();
        }

        var map = schema.AdditionalTypeInstances.ToDictionary(x => x.Name);

        IGraphType? Convert(IGraphType? type)
        {
            switch (type)
            {
                case GraphQLTypeReference reference:
                    return map.GetValueOrDefault(reference.TypeName) ?? reference;
                case NonNullGraphType nonNull:
                    return new NonNullGraphType(Convert(nonNull.ResolvedType));
                case ListGraphType list:
                    return new ListGraphType(Convert(list.ResolvedType));
                default:
                    return type;
            }
        }

        var result = new List<IGraphType>();

        foreach (var type in schema.AdditionalTypeInstances)
        {
            if (type is IComplexGraphType complexGraphType)
            {
                type.Name = typeNames[type.Name];

                foreach (var field in complexGraphType.Fields)
                {
                    // Assign a resolver to support json values.
                    field.Resolver = DynamicResolver.Instance;
                    field.ResolvedType = Convert(field.ResolvedType);
                }
            }

            result.Add(type);
        }

        return result.ToArray();
    }
}
