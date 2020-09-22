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
        public string? Format { get; set; }

        public int PixelWidth { get; }

        public int PixelHeight { get; }

        public bool IsRotatedOrSwapped { get; }

        public ImageInfo(int pixelWidth, int pixelHeight, bool isRotatedOrSwapped)
        {
            Guard.GreaterThan(pixelWidth, 0, nameof(pixelWidth));
            Guard.GreaterThan(pixelHeight, 0, nameof(pixelHeight));

            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;

            IsRotatedOrSwapped = isRotatedOrSwapped;
        }
    }
}
