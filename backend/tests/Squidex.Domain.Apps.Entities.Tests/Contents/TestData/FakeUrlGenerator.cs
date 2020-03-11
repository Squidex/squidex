// ==========================================================================
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

        public string? AssetThumbnail(Guid assetId, AssetType assetType)
        {
            return $"assets/{assetId}?width=100";
        }

        public string? AssetSource(Guid assetId, long fileVersion)
        {
            return $"assets/source/{assetId}";
        }

        public string AssetContent(Guid assetId)
        {
            return $"assets/{assetId}";
        }

        public string ContentUI(NamedId<Guid> appId, NamedId<Guid> schemaId, Guid contentId)
        {
            return $"contents/{schemaId.Name}/{contentId}";
        }

        public string AppSettingsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string AssetsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string AssetsUI(NamedId<Guid> appId, string? query = null)
        {
            throw new NotSupportedException();
        }

        public string AssetSource(Guid assetId)
        {
            throw new NotSupportedException();
        }

        public string BackupsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string ClientsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string ContentsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string ContentsUI(NamedId<Guid> appId, NamedId<Guid> schemaId)
        {
            throw new NotSupportedException();
        }

        public string ContributorsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string DashboardUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string LanguagesUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string PatternsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string PlansUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string RolesUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string RulesUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string SchemasUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string SchemaUI(NamedId<Guid> appId, NamedId<Guid> schemaId)
        {
            throw new NotSupportedException();
        }

        public string WorkflowsUI(NamedId<Guid> appId)
        {
            throw new NotSupportedException();
        }

        public string UI()
        {
            throw new NotSupportedException();
        }
    }
}
