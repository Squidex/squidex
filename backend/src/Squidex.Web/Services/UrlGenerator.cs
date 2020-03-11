// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Web.Services
{
    public sealed class UrlGenerator : IUrlGenerator
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly UrlsOptions urlsOptions;

        public bool CanGenerateAssetSourceUrl { get; }

        public UrlGenerator(IOptions<UrlsOptions> urlsOptions, IAssetFileStore assetFileStore, bool allowAssetSourceUrl)
        {
            Guard.NotNull(assetFileStore);
            Guard.NotNull(urlsOptions);

            this.assetFileStore = assetFileStore;

            this.urlsOptions = urlsOptions.Value;

            CanGenerateAssetSourceUrl = allowAssetSourceUrl;
        }

        public string? AssetThumbnail(Guid assetId, AssetType assetType)
        {
            if (assetType != AssetType.Image)
            {
                return null;
            }

            return urlsOptions.BuildUrl($"api/assets/{assetId}?width=100&mode=Max");
        }

        public string AppSettingsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings", false);
        }

        public string AssetContent(Guid assetId)
        {
            return urlsOptions.BuildUrl($"api/assets/{assetId}");
        }

        public string? AssetSource(Guid assetId, long fileVersion)
        {
            return assetFileStore.GeneratePublicUrl(assetId, fileVersion);
        }

        public string AssetsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/assets", false);
        }

        public string AssetsUI(NamedId<Guid> appId, string? query = null)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/assets?query={query}", false);
        }

        public string BackupsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/backups", false);
        }

        public string ClientsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/clients", false);
        }

        public string ContentsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/content", false);
        }

        public string ContentsUI(NamedId<Guid> appId, NamedId<Guid> schemaId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/content/{schemaId.Name}", false);
        }

        public string ContentUI(NamedId<Guid> appId, NamedId<Guid> schemaId, Guid contentId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/content/{schemaId.Name}/{contentId}/history", false);
        }

        public string ContributorsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/contributors", false);
        }

        public string DashboardUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}", false);
        }

        public string LanguagesUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/languages", false);
        }

        public string PatternsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/patterns", false);
        }

        public string PlansUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/plans", false);
        }

        public string RolesUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/roles", false);
        }

        public string RulesUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/rules", false);
        }

        public string SchemasUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/schemas", false);
        }

        public string SchemaUI(NamedId<Guid> appId, NamedId<Guid> schemaId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/schemas/{schemaId.Name}", false);
        }

        public string WorkflowsUI(NamedId<Guid> appId)
        {
            return urlsOptions.BuildUrl($"app/{appId.Name}/settings/workflows", false);
        }

        public string UI()
        {
            return urlsOptions.BuildUrl("app", false);
        }
    }
}
