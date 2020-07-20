﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.TestData
{
    public sealed class FakeUrlGenerator : IUrlGenerator
    {
        public bool CanGenerateAssetSourceUrl { get; } = true;

        public string? AssetThumbnail(NamedId<DomainId> appId, DomainId assetId, AssetType assetType)
        {
            return $"assets/{assetId}?width=100";
        }

        public string? AssetSource(NamedId<DomainId> appId, DomainId assetId, long fileVersion)
        {
            return $"assets/source/{assetId}";
        }

        public string AssetContent(NamedId<DomainId> appId, DomainId assetId)
        {
            return $"assets/{assetId}";
        }

        public string ContentUI(NamedId<DomainId> appId, NamedId<DomainId> schemaId, DomainId contentId)
        {
            return $"contents/{schemaId.Name}/{contentId}";
        }

        public string AppSettingsUI(NamedId<DomainId> appId)
        {
            throw new NotSupportedException();
        }

        public string AssetsUI(NamedId<DomainId> appId)
        {
            throw new NotSupportedException();
        }

        public string AssetsUI(NamedId<DomainId> appId, string? query = null)
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

        public string ContentsUI(NamedId<DomainId> appId)
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
}
