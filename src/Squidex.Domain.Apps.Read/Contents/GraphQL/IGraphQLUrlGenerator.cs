// ==========================================================================
//  IGraphQLUrlGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
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
