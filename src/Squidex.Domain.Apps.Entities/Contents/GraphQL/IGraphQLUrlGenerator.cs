// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public interface IGraphQLUrlGenerator
    {
        bool CanGenerateAssetSourceUrl { get; }

        string GenerateAssetUrl(IAppEntity app, IAssetEntity asset);

        string GenerateAssetThumbnailUrl(IAppEntity app, IAssetEntity asset);

        string GenerateAssetSourceUrl(IAppEntity app, IAssetEntity asset);

        string GenerateContentUrl(IAppEntity app, ISchemaEntity schema, IContentEntity content);
    }
}
