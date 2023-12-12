// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.TestHelpers;

public static class TestAssets
{
    public static Asset Document(DomainId id)
    {
        return new Asset
        {
            Id = id,
            FileName = "MyDocument.pdf",
            FileSize = 1024 * 4,
            Type = AssetType.Unknown
        };
    }

    public static Asset Image(DomainId id)
    {
        return new Asset
        {
            Id = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Image,
            Metadata =
                new AssetMetadata()
                    .SetPixelWidth(800)
                    .SetPixelHeight(600)
        };
    }

    public static Asset Video(DomainId id)
    {
        return new Asset
        {
            Id = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Video,
            Metadata =
                new AssetMetadata()
                    .SetVideoWidth(800)
                    .SetVideoHeight(600)
        };
    }

    public static Asset Svg(DomainId id)
    {
        return new Asset
        {
            Id = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Unknown,
            MimeType = "image/svg+xml"
        };
    }
}
