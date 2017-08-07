// ==========================================================================
//  GraphQLUrlGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure.Assets;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Pipeline
{
    public sealed class GraphQLUrlGenerator : IGraphQLUrlGenerator
    {
        private readonly IAssetStore assetStore;
        private readonly MyUrlsOptions urlsOptions;

        public bool CanGenerateAssetSourceUrl { get; }

        public GraphQLUrlGenerator(IOptions<MyUrlsOptions> urlsOptions, IAssetStore assetStore, bool allowAssetSourceUrl)
        {
            this.assetStore = assetStore;
            this.urlsOptions = urlsOptions.Value;

            CanGenerateAssetSourceUrl = allowAssetSourceUrl;
        }

        public string GenerateAssetThumbnailUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            if (!assetEntity.IsImage)
            {
                return null;
            }

            return urlsOptions.BuildUrl($"api/assets/{assetEntity.Id}?version={assetEntity.Version}&width=100&mode=Max");
        }

        public string GenerateAssetUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            return urlsOptions.BuildUrl($"api/assets/{assetEntity.Id}?version={assetEntity.Version}");
        }

        public string GenerateContentUrl(IAppEntity appEntity, ISchemaEntity schemaEntity, IContentEntity contentEntity)
        {
            return urlsOptions.BuildUrl($"api/content/{appEntity.Name}/{schemaEntity.Name}/{contentEntity.Id}");
        }

        public string GenerateAssetSourceUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            return assetStore.GenerateSourceUrl(assetEntity.Id.ToString(), assetEntity.FileVersion, null);
        }
    }
}
