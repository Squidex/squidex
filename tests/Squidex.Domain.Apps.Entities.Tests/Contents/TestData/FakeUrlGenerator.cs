// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.TestData
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
