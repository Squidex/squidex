// ==========================================================================
//  ImageInfo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    public sealed class ImageInfo
    {
        public int PixelWidth { get; }

        public int PixelHeight { get; }

        public ImageInfo(int pixelWidth, int pixelHeight)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }
    }
}
