// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public interface IUrlGenerator
    {
        bool CanGenerateAssetSourceUrl { get; }

        string? AssetSource(NamedId<DomainId> appId, DomainId assetId, long fileVersion);

        string? AssetThumbnail(NamedId<DomainId> appId, string idOrSlug, AssetType assetType);

        string AssetsUI(NamedId<DomainId> appId, string? query = null);

        string AssetContent(NamedId<DomainId> appId, string idOrSlug);

        string AssetContentBase();

        string AssetContentBase(string appName);

        string BackupsUI(NamedId<DomainId> appId);

        string ClientsUI(NamedId<DomainId> appId);

        string ContentsUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId);

        string ContentUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId, DomainId contentId);

        string ContributorsUI(NamedId<DomainId> appId);

        string DashboardUI(NamedId<DomainId> appId);

        string LanguagesUI(NamedId<DomainId> appId);

        string PatternsUI(NamedId<DomainId> appId);

        string PlansUI(NamedId<DomainId> appId);

        string RolesUI(NamedId<DomainId> appId);

        string RulesUI(NamedId<DomainId> appId);

        string SchemasUI(NamedId<DomainId> appId);

        string SchemaUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId);

        string WorkflowsUI(NamedId<DomainId> appId);

        string UI();
    }
}
