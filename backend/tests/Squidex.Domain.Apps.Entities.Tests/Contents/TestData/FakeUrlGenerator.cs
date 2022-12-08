// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.TestData;

public sealed class FakeUrlGenerator : IUrlGenerator
{
    public bool CanGenerateAssetSourceUrl { get; } = true;

    public string? AssetThumbnail(NamedId<DomainId> appId, string idOrSlug, AssetType assetType)
    {
        return $"assets/{appId.Name}/{idOrSlug}?width=100";
    }

    public string? AssetSource(NamedId<DomainId> appId, DomainId assetId, long fileVersion)
    {
        return $"assets/source/{assetId}";
    }

    public string AssetContent(NamedId<DomainId> appId, string idOrSlug)
    {
        return $"assets/{appId.Name}/{idOrSlug}";
    }

    public string ContentUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId, DomainId contentId)
    {
        return $"contents/{schemaId.Name}/{contentId}";
    }

    public string AssetContentBase()
    {
        return "$assets/";
    }

    public string AssetContentCDNBase()
    {
        return $"cdn/assets/";
    }

    public string ContentBase()
    {
        return $"contents/";
    }

    public string ContentCDNBase()
    {
        return $"cdn/contents/";
    }

    public string AssetsUI(NamedId<DomainId> appId, string? @ref = null)
    {
        throw new NotSupportedException();
    }

    public string AssetContentBase(string appName)
    {
        throw new NotSupportedException();
    }

    public string BackupsUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string ClientsUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string ContentsUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
    {
        throw new NotSupportedException();
    }

    public string ContributorsUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string DashboardUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string LanguagesUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string PatternsUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string PlansUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string RolesUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string Root()
    {
        throw new NotSupportedException();
    }

    public string RulesUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string SchemasUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string SchemaUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId)
    {
        throw new NotSupportedException();
    }

    public string WorkflowsUI(NamedId<DomainId> appId)
    {
        throw new NotSupportedException();
    }

    public string UI()
    {
        throw new NotSupportedException();
    }
}
