// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppQueriesGraphType : ObjectGraphType
    {
        public AppQueriesGraphType(IGraphQLContext ctx, IEnumerable<ISchemaEntity> schemas)
        {
            var assetType = ctx.GetAssetType();

            AddAssetFind(assetType);
            AddAssetsQueries(assetType);

            foreach (var schema in schemas)
            {
                var schemaName = schema.SchemaDef.Properties.Label.WithFallback(schema.SchemaDef.Name);
                var schemaType = ctx.GetSchemaType(schema.Id);

                AddContentFind(schema, schemaType, schemaName);
                AddContentQueries(ctx, schema, schemaType, schemaName);
            }

            Description = "The app queries.";
        }

        private void AddAssetFind(IGraphType assetType)
        {
            AddField(new FieldType
            {
                Name = "findAsset",
                Arguments = CreateAssetFindArguments(),
                ResolvedType = assetType,
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;
                    var contentId = Guid.Parse(c.GetArgument("id", Guid.Empty.ToString()));

                    return context.FindAssetAsync(contentId);
                }),
                Description = "Find an asset by id."
            });
        }

        private void AddContentFind(ISchemaEntity schema, IGraphType schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"find{schema.Name.ToPascalCase()}Content",
                Arguments = CreateContentFindTypes(schemaName),
                ResolvedType = schemaType,
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;
                    var contentId = Guid.Parse(c.GetArgument("id", Guid.Empty.ToString()));

                    return context.FindContentAsync(schema.Id, contentId);
                }),
                Description = $"Find an {schemaName} content by id."
            });
        }

        private void AddAssetsQueries(IGraphType assetType)
        {
            AddField(new FieldType
            {
                Name = "queryAssets",
                Arguments = CreateAssetQueryArguments(),
                ResolvedType = new ListGraphType(new NonNullGraphType(assetType)),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;

                    var argTop = c.GetArgument("top", 20);
                    var argSkip = c.GetArgument("skip", 0);
                    var argQuery = c.GetArgument("search", string.Empty);

                    return context.QueryAssetsAsync(argQuery, argSkip, argTop);
                }),
                Description = "Query assets items."
            });

            AddField(new FieldType
            {
                Name = "queryAssetsWithTotal",
                Arguments = CreateAssetQueryArguments(),
                ResolvedType = new AssetResultGraphType(assetType),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;

                    var argTop = c.GetArgument("top", 20);
                    var argSkip = c.GetArgument("skip", 0);
                    var argQuery = c.GetArgument("search", string.Empty);

                    return context.QueryAssetsAsync(argQuery, argSkip, argTop);
                }),
                Description = "Query assets items with total count."
            });
        }

        private void AddContentQueries(IGraphQLContext ctx, ISchemaEntity schema, IGraphType schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"query{schema.Name.ToPascalCase()}Contents",
                Arguments = CreateContentQueryArguments(),
                ResolvedType = new ListGraphType(new NonNullGraphType(schemaType)),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;
                    var contentQuery = BuildODataQuery(c);

                    return context.QueryContentsAsync(schema.Id.ToString(), contentQuery);
                }),
                Description = $"Query {schemaName} content items."
            });

            AddField(new FieldType
            {
                Name = $"query{schema.Name.ToPascalCase()}ContentsWithTotal",
                Arguments = CreateContentQueryArguments(),
                ResolvedType = new ContentResultGraphType(ctx, schema, schemaName),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (GraphQLQueryContext)c.UserContext;
                    var contentQuery = BuildODataQuery(c);

                    return context.QueryContentsAsync(schema.Id.ToString(), contentQuery);
                }),
                Description = $"Query {schemaName} content items with total count."
            });
        }

        private static QueryArguments CreateAssetFindArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "id",
                    Description = "The id of the asset.",
                    DefaultValue = string.Empty
                }
            };
        }

        private static QueryArguments CreateContentFindTypes(string schemaName)
        {
            return new QueryArguments
            {
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "id",
                    Description = $"The id of the {schemaName} content.",
                    DefaultValue = string.Empty
                }
            };
        }

        private static QueryArguments CreateAssetQueryArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "top",
                    Description = "Optional number of assets to take.",
                    DefaultValue = 20
                },
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "skip",
                    Description = "Optional number of assets to skip.",
                    DefaultValue = 0
                },
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "search",
                    Description = "Optional query.",
                    DefaultValue = string.Empty
                }
            };
        }

        private static QueryArguments CreateContentQueryArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "top",
                    Description = "Optional number of contents to take.",
                    DefaultValue = 20
                },
                new QueryArgument(typeof(IntGraphType))
                {
                    Name = "skip",
                    Description = "Optional number of contents to skip.",
                    DefaultValue = 0
                },
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "filter",
                    Description = "Optional OData filter.",
                    DefaultValue = string.Empty
                },
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "search",
                    Description = "Optional OData full text search.",
                    DefaultValue = string.Empty
                },
                new QueryArgument(typeof(StringGraphType))
                {
                    Name = "orderby",
                    Description = "Optional OData order definition.",
                    DefaultValue = string.Empty
                }
            };
        }

        private static string BuildODataQuery(ResolveFieldContext c)
        {
            var odataQuery = "?" +
                string.Join("&",
                    c.Arguments
                        .Select(x => new { x.Key, Value = x.Value.ToString() }).Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .Select(x => $"${x.Key}={x.Value}"));

            return odataQuery;
        }
    }
}
