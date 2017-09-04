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

        public string GenerateAssetThumbnailUrl(IAppEntity app, IAssetEntity asset)
        {
            if (!asset.IsImage)
            {
                return null;
            }

            return urlsOptions.BuildUrl($"api/assets/{asset.Id}?version={asset.Version}&width=100&mode=Max");
        }

        public string GenerateAssetUrl(IAppEntity app, IAssetEntity asset)
        {
            return urlsOptions.BuildUrl($"api/assets/{asset.Id}?version={asset.Version}");
        }

        public string GenerateContentUrl(IAppEntity app, ISchemaEntity schema, IContentEntity content)
        {
            return urlsOptions.BuildUrl($"api/content/{app.Name}/{schema.Name}/{content.Id}");
        }

        public string GenerateAssetSourceUrl(IAppEntity app, IAssetEntity asset)
        {
            return assetStore.GenerateSourceUrl(asset.Id.ToString(), asset.FileVersion, null);
        }
    }
}
