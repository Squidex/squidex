// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.TestHelpers;

public static class TestAssets
{
    public sealed class AssetInfo : IAssetInfo
    {
        public DomainId AssetId { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public string MimeType { get; set; }

        public string Slug { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public AssetMetadata Metadata { get; set; }

        public AssetType Type { get; set; }
    }

    public static AssetInfo Document(DomainId id)
    {
        return new AssetInfo
        {
            AssetId = id,
            FileName = "MyDocument.pdf",
            FileSize = 1024 * 4,
            Type = AssetType.Unknown
        };
    }

    public static AssetInfo Image(DomainId id)
    {
        return new AssetInfo
        {
            AssetId = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Image,
            Metadata =
                new AssetMetadata()
                    .SetPixelWidth(800)
                    .SetPixelHeight(600)
        };
    }

    public static AssetInfo Video(DomainId id)
    {
        return new AssetInfo
        {
            AssetId = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Video,
            Metadata =
                new AssetMetadata()
                    .SetVideoWidth(800)
                    .SetVideoHeight(600)
        };
    }

    public static AssetInfo Svg(DomainId id)
    {
        return new AssetInfo
        {
            AssetId = id,
            FileName = "MyImage.png",
            FileSize = 1024 * 8,
            Type = AssetType.Unknown,
            MimeType = "image/svg+xml"
        };
    }
}
