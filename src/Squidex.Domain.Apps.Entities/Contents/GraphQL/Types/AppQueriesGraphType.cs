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
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class AppQueriesGraphType : ObjectGraphType
    {
        private static readonly HashSet<string> AssetArgumentsToSkip = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "FolderId" };

        public AppQueriesGraphType(IGraphModel model, IEnumerable<ISchemaEntity> schemas)
        {
            var assetType = model.GetAssetType();

            AddAssetFind(assetType);
            AddAssetsQueries(assetType);

            foreach (var schema in schemas)
            {
                var schemaId = schema.Id;
                var schemaType = schema.TypeName();
                var schemaName = schema.DisplayName();

                var contentType = model.GetContentType(schema.Id);

                AddContentFind(schemaId, schemaType, schemaName, contentType);
                AddContentQueries(schemaId, schemaType, schemaName, contentType);
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

        private void AddContentFind(Guid schemaId, string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"find{schemaType}Content",
                Arguments = CreateContentFindTypes(schemaName),
                ResolvedType = contentType,
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentId = c.GetArgument<Guid>("id");

                    return e.FindContentAsync(schemaId, contentId);
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
                Resolver = ResolveAsync((c, e) =>
                {
                    var assetQuery = c.Arguments.ToOData(AssetArgumentsToSkip);
                    var assetFolder = c.GetArgument<Guid?>("folderId");

                    return e.QueryAssetsAsync(assetFolder, assetQuery);
                }),
                Description = "Get assets."
            });

            AddField(new FieldType
            {
                Name = "queryAssetsWithTotal",
                Arguments = CreateAssetQueryArguments(),
                ResolvedType = new AssetsResultGraphType(assetType),
                Resolver = ResolveAsync((c, e) =>
                {
                    var assetQuery = c.Arguments.ToOData(AssetArgumentsToSkip);
                    var assetFolder = c.GetArgument<Guid?>("folderId");

                    return e.QueryAssetsAsync(assetFolder, assetQuery);
                }),
                Description = "Get assets and total count."
            });
        }

        private void AddContentQueries(Guid schemaId, string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"query{schemaType}Contents",
                Arguments = CreateContentQueryArguments(),
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentQuery = c.Arguments.ToOData();

                    return e.QueryContentsAsync(schemaId.ToString(), contentQuery);
                }),
                Description = $"Query {schemaName} content items."
            });

            AddField(new FieldType
            {
                Name = $"query{schemaType}ContentsWithTotal",
                Arguments = CreateContentQueryArguments(),
                ResolvedType = new ContentsResultGraphType(schemaType, schemaName, contentType),
                Resolver = ResolveAsync((c, e) =>
                {
                    var contentQuery = c.Arguments.ToOData();

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

        private static QueryArguments CreateContentFindTypes(string schemaName)
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

        private static QueryArguments CreateAssetQueryArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "take",
                    Description = "Optional number of assets to take (Default: 20).",
                    DefaultValue = 20,
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
                    Name = "folderId",
                    Description = "Folder id.",
                    DefaultValue = null,
                    ResolvedType = AllTypes.Guid
                },
                new QueryArgument(AllTypes.None)
                {
                    Name = "search",
                    Description = "Optional query to limit the files by name.",
                    DefaultValue = string.Empty,
                    ResolvedType = AllTypes.String
                }
            };
        }

        private static QueryArguments CreateContentQueryArguments()
        {
            return new QueryArguments
            {
                new QueryArgument(AllTypes.None)
                {
                    Name = "top",
                    Description = "Optional number of contents to take (Default: 20).",
                    DefaultValue = 20,
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
