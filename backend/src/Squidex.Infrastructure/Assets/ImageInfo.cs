// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
