// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        public ImageSharpAssetThumbnailGenerator()
        {
            Configuration.Default.ImageFormatsManager.AddImageFormat(ImageFormats.Jpeg);
            Configuration.Default.ImageFormatsManager.AddImageFormat(ImageFormats.Png);
        }

        public Task CreateThumbnailAsync(Stream source, Stream destination, int? width, int? height, string mode)
        {
            return Task.Run(() =>
            {
                if (width == null && height == null)
                {
                    source.CopyTo(destination);

                    return;
                }

                if (!Enum.TryParse<ResizeMode>(mode, true, out var resizeMode))
                {
                    resizeMode = ResizeMode.Max;
                }

                var w = width ?? 0;
                var h = height ?? 0;

                using (var sourceImage = Image.Load(source, out var format))
                {
                    if (w >= sourceImage.Width && h >= sourceImage.Height && resizeMode == ResizeMode.Crop)
                    {
                        resizeMode = ResizeMode.BoxPad;
                    }

                    var options = new ResizeOptions { Size = new Size(w, h), Mode = resizeMode };

                    sourceImage.Mutate(x => x.Resize(options));
                    sourceImage.Save(destination, format);
                }
            });
        }

        public Task<ImageInfo> GetImageInfoAsync(Stream source)
        {
            return Task.Run(() =>
            {
                try
                {
                    var image = Image.Load(source);

                    return new ImageInfo(image.Width, image.Height);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
