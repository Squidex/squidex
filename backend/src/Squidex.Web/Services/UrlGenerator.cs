﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Web.Services
{
    public sealed class UrlGenerator : IGraphQLUrlGenerator, IRuleUrlGenerator, IAssetUrlGenerator, IEmailUrlGenerator
    {
        private readonly IAssetStore assetStore;
        private readonly UrlsOptions urlsOptions;

        public bool CanGenerateAssetSourceUrl { get; }

        public UrlGenerator(IOptions<UrlsOptions> urlsOptions, IAssetStore assetStore, bool allowAssetSourceUrl)
        {
            this.assetStore = assetStore;
            this.urlsOptions = urlsOptions.Value;

            CanGenerateAssetSourceUrl = allowAssetSourceUrl;
        }

        public string? GenerateAssetThumbnailUrl(IAppEntity app, IAssetEntity asset)
        {
            if (!asset.IsImage)
            {
                return null;
            }

            return urlsOptions.BuildUrl($"api/assets/{asset.Id}?version={asset.Version}&width=100&mode=Max");
        }

        public string GenerateUrl(string assetId)
        {
            return urlsOptions.BuildUrl($"api/assets/{assetId}");
        }

        public string GenerateAssetUrl(IAppEntity app, IAssetEntity asset)
        {
            return urlsOptions.BuildUrl($"api/assets/{asset.Id}?version={asset.Version}");
        }

        public string GenerateContentUrl(IAppEntity app, ISchemaEntity schema, IContentEntity content)
        {
            return urlsOptions.BuildUrl($"api/content/{app.Name}/{schema.SchemaDef.Name}/{content.Id}");
        }

        public string GenerateContentUIUrl(NamedId<Guid> appId, NamedId<Guid> schemaId, Guid contentId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/content/{schemaId.Name}/{contentId}/history");
        }

        public string GenerateUIUrl()
        {
            return urlsOptions.BuildUrl("app/");
        }

        public string? GenerateAssetSourceUrl(IAppEntity app, IAssetEntity asset)
        {
            return assetStore.GeneratePublicUrl(asset.Id.ToString(), asset.FileVersion, null);
        }
    }
}
