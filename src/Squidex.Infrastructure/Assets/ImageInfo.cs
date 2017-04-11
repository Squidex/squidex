// ==========================================================================
//  ImageInfo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.d
// ==========================================================================

namespace Squidex.Infrastructure.Assets
{
    public sealed class ImageInfo
    {
        public int PixelWidth { get; }

        public int PixelHeight { get; }

        public ImageInfo(int pixelWidth, int pixelHeight)
        {
            Guard.GreaterThan(pixelWidth, 0, nameof(pixelWidth));
            Guard.GreaterThan(pixelHeight, 0, nameof(pixelHeight));

            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }
    }
}
