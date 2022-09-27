// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Dynamic
{
    internal static class DynamicSchemaBuilder
    {
        public static IGraphType[] ParseTypes(string? typeDefinitions, TypeNames names)
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

            if (!schema.AdditionalTypeInstances.Any())
            {
                return Array.Empty<IGraphType>();
            }

            Dictionary<string, string>? nameMap = null;

            var result = schema.AdditionalTypeInstances.ToArray();

            foreach (var type in result)
            {
                var newName = names[type.Name];

                if (!string.Equals(newName, type.Name, StringComparison.Ordinal))
                {
                    nameMap ??= new Dictionary<string, string>();
                    nameMap[type.Name] = newName;

                    type.Name = newName;
                }
            }

            foreach (var type in result)
            {
                if (type is IComplexGraphType complexGraphType)
                {
                    foreach (var field in complexGraphType.Fields)
                    {
                        field.Resolver = DynamicResolver.Instance;

                        if (nameMap != null)
                        {
                            if (field.ResolvedType is GraphQLTypeReference reference && nameMap.TryGetValue(reference.Name, out var newName))
                            {
                                field.ResolvedType = new GraphQLTypeReference(newName);
                            }
                        }
                    }
                }

                type.Name = names[type.Name];
            }

            return result;
        }
    }
}
