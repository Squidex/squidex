// ==========================================================================
//  FakeUrlGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.TestData
{
    public sealed class FakeUrlGenerator : IGraphQLUrlGenerator
    {
        public bool CanGenerateAssetSourceUrl { get; } = true;

        public string GenerateAssetUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            return $"assets/{assetEntity.Id}";
        }

        public string GenerateAssetThumbnailUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            return $"assets/{assetEntity.Id}?width=100";
        }

        public string GenerateAssetSourceUrl(IAppEntity appEntity, IAssetEntity assetEntity)
        {
            return $"assets/source/{assetEntity.Id}";
        }

        public string GenerateContentUrl(IAppEntity appEntity, ISchemaEntity schemaEntity, IContentEntity contentEntity)
        {
            return $"contents/{schemaEntity.Name}/{contentEntity.Id}";
        }
    }
}
