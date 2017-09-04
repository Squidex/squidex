// ==========================================================================
//  GraphModelType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Squidex.Domain.Apps.Read.Contents.GraphQL.Types
{
    public sealed class ContentQueryGraphType : ObjectGraphType
    {
        public ContentQueryGraphType(IGraphQLContext graphQLContext, IEnumerable<ISchemaEntity> schemas)
        {
            AddAssetFind(graphQLContext);
            AddAssetsQuery(graphQLContext);

            foreach (var schema in schemas)
            {
                var schemaName = schema.SchemaDef.Properties.Label.WithFallback(schema.SchemaDef.Name);
                var schemaType = graphQLContext.GetSchemaType(schema.Id);

                AddContentFind(schema, schemaType, schemaName);
                AddContentQuery(schema, schemaType, schemaName);
            }

            Description = "The app queries.";
        }

        private void AddAssetFind(IGraphQLContext graphQLContext)
        {
            AddField(new FieldType
            {
                Name = "findAsset",
                Arguments = new QueryArguments
                {
                    new QueryArgument(typeof(StringGraphType))
                    {
                        Name = "id",
                        Description = "The id of the asset.",
                        DefaultValue = string.Empty
                    }
                },
                ResolvedType = graphQLContext.GetAssetType(),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (QueryContext)c.UserContext;
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
                Arguments = new QueryArguments
                {
                    new QueryArgument(typeof(StringGraphType))
                    {
                        Name = "id",
                        Description = $"The id of the {schemaName} content.",
                        DefaultValue = string.Empty
                    }
                },
                ResolvedType = schemaType,
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (QueryContext)c.UserContext;
                    var contentId = Guid.Parse(c.GetArgument("id", Guid.Empty.ToString()));

                    return context.FindContentAsync(schema.Id, contentId);
                }),
                Description = $"Find an {schemaName} content by id."
            });
        }

        private void AddAssetsQuery(IGraphQLContext graphQLContext)
        {
            AddField(new FieldType
            {
                Name = "queryAssets",
                Arguments = new QueryArguments
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
                },
                ResolvedType = new ListGraphType(new NonNullGraphType(graphQLContext.GetAssetType())),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (QueryContext)c.UserContext;

                    var argTop = c.GetArgument("top", 20);
                    var argSkip = c.GetArgument("skip", 0);
                    var argQuery = c.GetArgument("search", string.Empty);

                    return context.QueryAssetsAsync(argQuery, argSkip, argTop);
                }),
                Description = "Query assets items."
            });
        }

        private void AddContentQuery(ISchemaEntity schema, IGraphType schemaType, string schemaName)
        {
            AddField(new FieldType
            {
                Name = $"query{schema.Name.ToPascalCase()}Contents",
                Arguments = new QueryArguments
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
                },
                ResolvedType = new ListGraphType(new NonNullGraphType(schemaType)),
                Resolver = new FuncFieldResolver<object>(c =>
                {
                    var context = (QueryContext)c.UserContext;
                    var contentQuery = BuildODataQuery(c);

                    return context.QueryContentsAsync(schema.Id, contentQuery);
                }),
                Description = $"Query {schemaName} content items."
            });
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
