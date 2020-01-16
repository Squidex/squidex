// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppQueriesGraphType : ObjectGraphType
    {
        public AppQueriesGraphType(IGraphModel model, int pageSizeContents, int pageSizeAssets, IEnumerable<ISchemaEntity> schemas)
        {
            var assetType = model.GetAssetType();

            AddAssetFind(assetType);
            AddAssetsQueries(assetType, pageSizeAssets);

            foreach (var schema in schemas)
            {
                var schemaId = schema.Id;
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);

                AddContentFind(schemaType, schemaName, contentType);
                AddContentQueries(schemaId, schemaType, schemaName, contentType, pageSizeContents);
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
                Resolver = ResolveAsync((c, e) =>
                {
                    var assetId = c.GetArgument<Guid>("id");

                    return e.FindAssetAsync(assetId);
                }),
                Description = "Find an asset by id."
            });
        }

        private void AddContentFind(string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"find{schemaType}Content",
                Arguments = CreateContentFindArguments(schemaName),
                ResolvedType = contentType,
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    return e.FindContentAsync(contentId);
                }),
                Description = $"Find an {schemaName} content by id."
            });
        }

        private void AddAssetsQueries(IGraphType assetType, int pageSize)
        {
            AddField(new FieldType
            {
                Name = "queryAssets",
                Arguments = CreateAssetQueryArguments(pageSize),
                ResolvedType = new ListGraphType(new NonNullGraphType(assetType)),
                Resolver = ResolveAsync((c, e) =>
                {
                    var assetQuery = BuildODataQuery(c);

                    return e.QueryAssetsAsync(assetQuery);
                }),
                Description = "Get assets."
            });

            AddField(new FieldType
            {
                Name = "queryAssetsWithTotal",
                Arguments = CreateAssetQueryArguments(pageSize),
                ResolvedType = new AssetsResultGraphType(assetType),
                Resolver = ResolveAsync((c, e) =>
                {
                    var assetQuery = BuildODataQuery(c);

                    return e.QueryAssetsAsync(assetQuery);
                }),
                Description = "Get assets and total count."
            });
        }

        private void AddContentQueries(Guid schemaId, string schemaType, string schemaName, IGraphType contentType, int pageSize)
        {
            AddField(new FieldType
            {
                Name = $"query{schemaType}Contents",
                Arguments = CreateContentQueryArguments(pageSize),
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentQuery = BuildODataQuery(c);

                    return e.QueryContentsAsync(schemaId.ToString(), contentQuery);
                }),
                Description = $"Query {schemaName} content items."
            });

            AddField(new FieldType
            {
                Name = $"query{schemaType}ContentsWithTotal",
                Arguments = CreateContentQueryArguments(pageSize),
                ResolvedType = new ContentsResultGraphType(schemaType, schemaName, contentType),
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentQuery = BuildODataQuery(c);

                    return e.QueryContentsAsync(schemaId.ToString(), contentQuery);
                }),
                Description = $"Query {schemaName} content items with total count."
            });
        }

        private static QueryArguments CreateAssetFindArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = "The id of the asset (GUID).",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                }
            };
        }

        private static QueryArguments CreateContentFindArguments(string schemaName)
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "id",
                    Description = $"The id of the {schemaName} content (GUID)",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.NonNullGuid
                }
            };
        }

        private static QueryArguments CreateAssetQueryArguments(int pageSize)
        {
            return new QueryArguments
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

        private static QueryArguments CreateContentQueryArguments(int pageSize)
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "top",
                    Description = $"Optional number of contents to take (Default: {pageSize}).",
                    DefaultValue = pageSize,
                    ResolvedType = AllTypes.Int
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "skip",
                    Description = "Optional number of contents to skip.",
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
                    Name = "search",
                    Description = "Optional OData full text search.",
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

        private static string BuildODataQuery(ResolveFieldContext c)
        {
            var odataQuery = "?" +
                string.Join("&",
                    c.Arguments
                        .Select(x => new { x.Key, Value = x.Value.ToString() }).Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .Select(x => $"${x.Key}={x.Value}"));

            return odataQuery;
        }

        private static IFieldResolver ResolveAsync<T>(Func<ResolveFieldContext, GraphQLExecutionContext, Task<T>> action)
        {
            return new FuncFieldResolver<Task<T>>(c =>
            {
                var e = (GraphQLExecutionContext)c.UserContext;

                return action(c, e);
            });
        }
    }
}
