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
using SixLabors.Primitives;
using ISResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using ISResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        public Task CreateThumbnailAsync(Stream source, Stream destination, ResizeOptions options)
        {
            Guard.NotNull(options);

            return Task.Run(() =>
            {
                var w = options.Width ?? 0;
                var h = options.Height ?? 0;

                if (w <= 0 && h <= 0 && !options.Quality.HasValue)
                {
                    source.CopyTo(destination);

                    return;
                }

                using (var sourceImage = Image.Load(source, out var format))
                {
                    var encoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);

                    if (options.Quality.HasValue)
                    {
                        encoder = new JpegEncoder { Quality = options.Quality.Value };
                    }

                    if (encoder == null)
                    {
                        throw new NotSupportedException();
                    }

                    if (w > 0 || h > 0)
                    {
                        var isCropUpsize = options.Mode == ResizeMode.CropUpsize;

                        if (!Enum.TryParse<ISResizeMode>(options.Mode.ToString(), true, out var resizeMode))
                        {
                            resizeMode = ISResizeMode.Max;
                        }

                        if (isCropUpsize)
                        {
                            resizeMode = ISResizeMode.Crop;
                        }

                        if (w >= sourceImage.Width && h >= sourceImage.Height && resizeMode == ISResizeMode.Crop && !isCropUpsize)
                        {
                            resizeMode = ISResizeMode.BoxPad;
                        }

                        var resizeOptions = new ISResizeOptions { Size = new Size(w, h), Mode = resizeMode };

                        if (options.FocusX.HasValue && options.FocusY.HasValue)
                        {
                            resizeOptions.CenterCoordinates = new float[]
                            {
                                +(options.FocusX.Value / 2f) + 0.5f,
                                -(options.FocusX.Value / 2f) + 0.5f
                            };
                        }

                        sourceImage.Mutate(x => x.Resize(resizeOptions));
                    }

                    sourceImage.Save(destination, encoder);
                }
            });
        }

        public Task<ImageInfo?> GetImageInfoAsync(Stream source)
        {
            ImageInfo? result = null;

            try
            {
                var image = Image.Identify(source);

                if (image != null)
                {
                    result = new ImageInfo(image.Width, image.Height);
                }
            }
            catch
            {
                result = null;
            }

            return Task.FromResult(result);
        }
    }
}
