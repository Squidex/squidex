// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core
{
    public interface IUrlGenerator
    {
        bool CanGenerateAssetSourceUrl { get; }

        string? AssetSource(Guid assetId, long fileVersion);

        string? AssetThumbnail(Guid assetId, AssetType assetType);

        string AppSettingsUI(NamedId<Guid> appId);

        string AssetsUI(NamedId<Guid> appId);

        string AssetsUI(NamedId<Guid> appId, string? query = null);

        string AssetContent(Guid assetId);

        string BackupsUI(NamedId<Guid> appId);

        string ClientsUI(NamedId<Guid> appId);

        string ContentsUI(NamedId<Guid> appId);

        string ContentsUI(NamedId<Guid> appId, NamedId<Guid> schemaId);

        string ContentUI(NamedId<Guid> appId, NamedId<Guid> schemaId, Guid contentId);

        string ContributorsUI(NamedId<Guid> appId);

        string DashboardUI(NamedId<Guid> appId);

        string LanguagesUI(NamedId<Guid> appId);

        string PatternsUI(NamedId<Guid> appId);

        string PlansUI(NamedId<Guid> appId);

        string RolesUI(NamedId<Guid> appId);

        string RulesUI(NamedId<Guid> appId);

        string SchemasUI(NamedId<Guid> appId);

        string SchemaUI(NamedId<Guid> appId, NamedId<Guid> schemaId);

        string WorkflowsUI(NamedId<Guid> appId);

        string UI();
    }
}
