﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Assets;

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
                    Description = "The id of the asset (GUID).",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                }
            };

            public static readonly IFieldResolver Resolver = new FuncFieldResolver<object?>(c =>
            {
                var id = c.GetArgument<Guid>("id");

                return ((GraphQLExecutionContext)c.UserContext).FindAssetAsync(id);
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
                        DefaultValue = string.Empty,
                        ResolvedType = AllTypes.String
                    },
                    new QueryArgument(AllTypes.None)
                    {
                        Name = "orderby",
                        Description = "Optional OData order definition.",
                        DefaultValue = string.Empty,
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
