// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using IGenericUrlGenerator = Squidex.Hosting.IUrlGenerator;

namespace Squidex.Web.Services
{
    public sealed class UrlGenerator : IUrlGenerator
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly IGenericUrlGenerator urlGenerator;

        public bool CanGenerateAssetSourceUrl { get; }

        public UrlGenerator(IGenericUrlGenerator urlGenerator, IAssetFileStore assetFileStore, bool allowAssetSourceUrl)
        {
            this.assetFileStore = assetFileStore;

            this.urlGenerator = urlGenerator;

            CanGenerateAssetSourceUrl = allowAssetSourceUrl;
        }

        public string? AssetThumbnail(NamedId<DomainId> appId, string idOrSlug, AssetType assetType)
        {
            if (assetType != AssetType.Image)
            {
                return null;
            }

            return urlGenerator.BuildUrl($"api/assets/{appId.Name}/{idOrSlug}?width=100&mode=Max");
        }

        public string AssetContentBase()
        {
            return urlGenerator.BuildUrl("api/assets/");
        }

        public string AssetContentBase(string appName)
        {
            return urlGenerator.BuildUrl($"api/assets/{appName}/");
        }

        public string AssetContent(NamedId<DomainId> appId, DomainId assetId)
        {
            return urlGenerator.BuildUrl($"api/assets/{appId.Name}/{assetId}");
        }

        public string AssetContent(NamedId<DomainId> appId, string idOrSlug)
        {
            return urlGenerator.BuildUrl($"api/assets/{appId.Name}/{idOrSlug}");
        }

        public string? AssetSource(NamedId<DomainId> appId, DomainId assetId, long fileVersion)
        {
            return assetFileStore.GeneratePublicUrl(appId.Id, assetId, fileVersion, null);
        }

        public string AssetsUI(NamedId<DomainId> appId, string? query = null)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/assets", false) + query != null ? $"?query={query}" : string.Empty;
        }

        public string BackupsUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/backups", false);
        }

        public string ClientsUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/clients", false);
        }

        public string ContentsUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/content/{schemaId.Name}", false);
        }

        public string ContentUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId, DomainId contentId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/content/{schemaId.Name}/{contentId}/history", false);
        }

        public string ContributorsUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/contributors", false);
        }

        public string DashboardUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}", false);
        }

        public string LanguagesUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/languages", false);
        }

        public string PatternsUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/patterns", false);
        }

        public string PlansUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/plans", false);
        }

        public string RolesUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/roles", false);
        }

        public string RulesUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/rules", false);
        }

        public string SchemasUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/schemas", false);
        }

        public string SchemaUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/schemas/{schemaId.Name}", false);
        }

        public string WorkflowsUI(NamedId<DomainId> appId)
        {
            return urlGenerator.BuildUrl($"app/{appId.Name}/settings/workflows", false);
        }

        public string UI()
        {
            return urlGenerator.BuildUrl("app", false);
        }
    }
}
