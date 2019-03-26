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
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        public Task CreateThumbnailAsync(Stream source, Stream destination, int? width = null, int? height = null, string mode = null, int? quality = null)
        {
            return Task.Run(() =>
            {
                if (!width.HasValue && !height.HasValue && !quality.HasValue)
                {
                    source.CopyTo(destination);

                    return;
                }

                using (var sourceImage = Image.Load(source, out var format))
                {
                    var encoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);

                    if (quality.HasValue)
                    {
                        encoder = new JpegEncoder { Quality = quality.Value };
                    }

                    if (encoder == null)
                    {
                        throw new NotSupportedException();
                    }

                    if (width.HasValue || height.HasValue)
                    {
                        var isCropUpsize = string.Equals("CropUpsize", mode, StringComparison.OrdinalIgnoreCase);

                        if (!Enum.TryParse<ResizeMode>(mode, true, out var resizeMode))
                        {
                            resizeMode = ResizeMode.Max;
                        }

                        if (isCropUpsize)
                        {
                            resizeMode = ResizeMode.Crop;
                        }

                        var resizeWidth = width ?? 0;
                        var resizeHeight = height ?? 0;

                        if (resizeWidth >= sourceImage.Width && resizeHeight >= sourceImage.Height && resizeMode == ResizeMode.Crop && !isCropUpsize)
                        {
                            resizeMode = ResizeMode.BoxPad;
                        }

                        var options = new ResizeOptions { Size = new Size(resizeWidth, resizeHeight), Mode = resizeMode };

                        sourceImage.Mutate(x => x.Resize(options));
                    }

                    sourceImage.Save(destination, encoder);
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
