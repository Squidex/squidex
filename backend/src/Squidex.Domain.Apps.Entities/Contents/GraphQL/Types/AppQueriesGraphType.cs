// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
                Arguments = AssetArguments.Find,
                ResolvedType = assetType,
                Resolver = AssetResolvers.FindAsset,
                Description = "Find an asset by id."
            });
        }

        private void AddContentFind(string schemaType, string schemaName, IGraphType contentType)
        {
            AddField(new FieldType
            {
                Name = $"find{schemaType}Content",
                Arguments = ContentArguments.Find,
                ResolvedType = contentType,
                Resolver = ContentResolvers.FindContent,
                Description = $"Find an {schemaName} content by id."
            });
        }

        private void AddAssetsQueries(IGraphType assetType, int pageSize)
        {
            var resolver = AssetResolvers.QueryAssets;

            AddField(new FieldType
            {
                Name = "queryAssets",
                Arguments = AssetArguments.Query(pageSize),
                ResolvedType = new ListGraphType(new NonNullGraphType(assetType)),
                Resolver = resolver,
                Description = "Get assets."
            });

            AddField(new FieldType
            {
                Name = "queryAssetsWithTotal",
                Arguments = AssetArguments.Query(pageSize),
                ResolvedType = new AssetsResultGraphType(assetType),
                Resolver = resolver,
                Description = "Get assets and total count."
            });
        }

        private void AddContentQueries(Guid schemaId, string schemaType, string schemaName, IGraphType contentType, int pageSize)
        {
            var resolver = ContentResolvers.QueryContents(schemaId);

            AddField(new FieldType
            {
                Name = $"query{schemaType}Contents",
                Arguments = ContentArguments.Query(pageSize),
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = resolver,
                Description = $"Query {schemaName} content items."
            });

            AddField(new FieldType
            {
                Name = $"query{schemaType}ContentsWithTotal",
                Arguments = ContentArguments.Query(pageSize),
                ResolvedType = new ContentsResultGraphType(schemaType, schemaName, contentType),
                Resolver = resolver,
                Description = $"Query {schemaName} content items with total count."
            });
        }
    }
}
