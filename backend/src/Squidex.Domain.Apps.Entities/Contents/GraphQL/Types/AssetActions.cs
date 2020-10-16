// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class AssetActions
    {
        public static class Metadata
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "path",
                    Description = "The path to the json value",
                    DefaultValue = null,
                    ResolvedType = AllTypes.String
                }
            };

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<IEnrichedAssetEntity, object?>(c =>
            {
                if (c.Arguments.TryGetValue("path", out var path))
                {
                    c.Source.Metadata.TryGetByPath(path as string, out var result);

                    return result;
                }

                return c.Source.Metadata;
            });
        }

        public static class Find
        {
            public static readonly QueryArguments Arguments = new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the asset (usually GUID).",
                    DefaultValue = null,
                    ResolvedType = AllTypes.NonNullDomainId
                }
            };

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<object?>(c =>
            {
                var assetId = c.GetArgument<DomainId>("id");

                return ((GraphQLExecutionContext)c.UserContext).FindAssetAsync(assetId);
            });
        }

        public static class Query
        {
            private static QueryArguments? resolver;

            public static QueryArguments Arguments(int pageSize)
            {
                return resolver ??= new QueryArguments
                {
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "top",
                        Description = $"Optional number of assets to take (Default: {pageSize}).",
                        DefaultValue = pageSize,
                        ResolvedType = AllTypes.Int
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "skip",
                        Description = "Optional number of assets to skip.",
                        DefaultValue = 0,
                        ResolvedType = AllTypes.Int
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "filter",
                        Description = "Optional OData filter.",
                        DefaultValue = null,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "orderby",
                        Description = "Optional OData order definition.",
                        DefaultValue = null,
                        ResolvedType = AllTypes.String
                    }
                };
            }

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<object?>(c =>
            {
                var query = c.BuildODataQuery();

                return ((GraphQLExecutionContext)c.UserContext).QueryAssetsAsync(query);
            });
        }
    }
}
