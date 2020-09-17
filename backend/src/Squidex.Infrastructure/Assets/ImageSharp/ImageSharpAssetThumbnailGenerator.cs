// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using ISResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using ISResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(Math.Max(Environment.ProcessorCount / 4, 1));

        public async Task CreateThumbnailAsync(Stream source, Stream destination, ResizeOptions options)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(destination, nameof(destination));
            Guard.NotNull(options, nameof(options));

            if (!options.IsValid)
            {
                source.CopyTo(destination);

                return;
            }

            var w = options.Width ?? 0;
            var h = options.Height ?? 0;

            await semaphoreSlim.WaitAsync();

            try
            {
                using (var image = Image.Load(source, out var format))
                {
                    var encoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);

                    if (encoder == null)
                    {
                        throw new NotSupportedException();
                    }

                    if (options.Quality.HasValue && (encoder is JpegEncoder || !options.KeepFormat))
                    {
                        encoder = new JpegEncoder { Quality = options.Quality.Value };
                    }

                    image.Mutate(x => x.AutoOrient());

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

                        if (w >= image.Width && h >= image.Height && resizeMode == ISResizeMode.Crop && !isCropUpsize)
                        {
                            resizeMode = ISResizeMode.BoxPad;
                        }

                        var resizeOptions = new ISResizeOptions { Size = new Size(w, h), Mode = resizeMode };

                        if (options.FocusX.HasValue && options.FocusY.HasValue)
                        {
                            resizeOptions.CenterCoordinates = new PointF(
                                +(options.FocusX.Value / 2f) + 0.5f,
                                -(options.FocusY.Value / 2f) + 0.5f
                            );
                        }

                        image.Mutate(x => x.Resize(resizeOptions));
                    }

                    image.Save(destination, encoder);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public Task<ImageInfo?> GetImageInfoAsync(Stream source)
        {
            Guard.NotNull(source, nameof(source));

            ImageInfo? result = null;

            try
            {
                var image = Image.Identify(source);

                if (image != null)
                {
                    result = GetImageInfo(image);
                }
            }
            catch
            {
                result = null;
            }

            return Task.FromResult(result);
        }

        public async Task<ImageInfo> FixOrientationAsync(Stream source, Stream destination)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(destination, nameof(destination));

            await semaphoreSlim.WaitAsync();

            try
            {
                using (var image = Image.Load(source, out var format))
                {
                    var encoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);

                    if (encoder == null)
                    {
                        throw new NotSupportedException();
                    }

                    image.Mutate(x => x.AutoOrient());

                    image.Save(destination, encoder);

                    return GetImageInfo(image);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private static ImageInfo GetImageInfo(IImageInfo image)
        {
            var isRotatedOrSwapped = false;

            if (image.Metadata.ExifProfile != null)
            {
                var value = image.Metadata.ExifProfile.GetValue(ExifTag.Orientation);

                isRotatedOrSwapped = value?.Value > 1;
            }

            return new ImageInfo(image.Width, image.Height, isRotatedOrSwapped);
        }
    }
}
