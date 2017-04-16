// ==========================================================================
//  ImageSharpAssetThumbnailGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        public ImageSharpAssetThumbnailGenerator()
        {
            Configuration.Default.AddImageFormat(new JpegFormat());
            Configuration.Default.AddImageFormat(new PngFormat());
        }

        public Task CreateThumbnailAsync(Stream source, Stream destination, int? width, int? height, string mode)
        {
            return Task.Run(() =>
            {
                if (width == null && height == null)
                {
                    source.CopyTo(destination);
                }

                if (!Enum.TryParse<ResizeMode>(mode, true, out var resizeMode))
                {
                    resizeMode = ResizeMode.Max;
                }

                var w = width ?? int.MaxValue;
                var h = height ?? int.MaxValue;

                using (var sourceImage = Image.Load(source))
                {
                    if (w >= sourceImage.Width && h >= sourceImage.Height && resizeMode == ResizeMode.Crop)
                    {
                        resizeMode = ResizeMode.BoxPad;
                    }

                    var options =
                        new ResizeOptions
                        {
                            Size = new Size(w, h),
                            Mode = resizeMode
                        };

                    sourceImage.MetaData.Quality = 0;
                    sourceImage.Resize(options).Save(destination);
                }
            });
        }

        public Task<ImageInfo> GetImageInfoAsync(Stream source)
        {
            return Task.Run(() =>
            {
                ImageInfo imageInfo = null;
                try
                {
                    var image = Image.Load(source);

                    if (image.Width > 0 && image.Height > 0)
                    {
                        imageInfo = new ImageInfo(image.Width, image.Height);
                    }
                }
                catch
                {
                    imageInfo = null;
                }

                return imageInfo;
            });
        }
    }
}
