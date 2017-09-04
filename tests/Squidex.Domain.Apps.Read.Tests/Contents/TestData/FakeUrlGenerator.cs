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

        public string GenerateAssetUrl(IAppEntity app, IAssetEntity asset)
        {
            return $"assets/{asset.Id}";
        }

        public string GenerateAssetThumbnailUrl(IAppEntity app, IAssetEntity asset)
        {
            return $"assets/{asset.Id}?width=100";
        }

        public string GenerateAssetSourceUrl(IAppEntity app, IAssetEntity asset)
        {
            return $"assets/source/{asset.Id}";
        }

        public string GenerateContentUrl(IAppEntity app, ISchemaEntity schema, IContentEntity content)
        {
            return $"contents/{schema.Name}/{content.Id}";
        }
    }
}
