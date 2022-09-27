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
        public static IGraphType[] ParseTypes(string? typeDefinitions)
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

            var result = schema.AdditionalTypeInstances.ToArray();

            foreach (var type in result)
            {
                if (type is IComplexGraphType complexGraphType)
                {
                    foreach (var field in complexGraphType.Fields)
                    {
                        // Assign a resolver to support json values.
                        field.Resolver = DynamicResolver.Instance;
                    }
                }

                // The names could have conflicts with other types in the schema. Therefore mark them as dynamic to resolve them later.
                DynamicNameVisitor.MarkDynamic(type);
            }

            return result;
        }
    }
}
