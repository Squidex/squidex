// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Assets;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public class GraphQLTypeFactory
    {
        private readonly Lazy<IGraphType> asset;
        private readonly Lazy<IGraphType> assetsList;
        private readonly Lazy<IGraphType> assetsResult;
        private readonly Lazy<FieldType> findAsset;
        private readonly Lazy<FieldType> queryAssets;
        private readonly Lazy<FieldType> queryAssetsWithTotal;

        public IGraphType Asset => asset.Value;

        public IGraphType AssetsList => assetsList.Value;

        public IGraphType AssetsResult => assetsResult.Value;

        public FieldType FindAsset => findAsset.Value;

        public FieldType QueryAssets => queryAssets.Value;

        public FieldType QueryAssetsWithTotal => queryAssetsWithTotal.Value;

        public GraphQLTypeFactory(IUrlGenerator urlGenerator)
        {
            asset = new Lazy<IGraphType>(() =>
            {
                return new AssetGraphType(urlGenerator.CanGenerateAssetSourceUrl);
            });

            assetsList = new Lazy<IGraphType>(() =>
            {
                return new NonNullGraphType(new ListGraphType(new NonNullGraphType(Asset)));
            });

            assetsResult = new Lazy<IGraphType>(() =>
            {
                return new AssetsResultGraphType(AssetsList);
            });

            findAsset = new Lazy<FieldType>(() =>
            {
                return new FieldType
                {
                    Name = "findAsset",
                    Arguments = AssetActions.Find.Arguments,
                    ResolvedType = Asset,
                    Resolver = AssetActions.Find.Resolver,
                    Description = "Find an asset by id."
                };
            });

            queryAssets = new Lazy<FieldType>(() =>
            {
                return new FieldType
                {
                    Name = "queryAssets",
                    Arguments = AssetActions.Query.Arguments,
                    ResolvedType = AssetsList,
                    Resolver = AssetActions.Query.Resolver,
                    Description = "Get assets."
                };
            });

            queryAssetsWithTotal = new Lazy<FieldType>(() =>
            {
                return new FieldType
                {
                    Name = "queryAssetsWithTotal",
                    Arguments = AssetActions.Query.Arguments,
                    ResolvedType = AssetsResult,
                    Resolver = AssetActions.Query.Resolver,
                    Description = "Get assets and total count."
                };
            });
        }
    }
}
