// ==========================================================================
//  ImageSharpAssetThumbnailGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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

        public Task<Stream> GetThumbnailOrNullAsync(Stream input, int dimension)
        {
            return Task.Run(() =>
            {
                var result = new MemoryStream();

                var options =
                    new ResizeOptions
                    {
                        Size = new Size(dimension, dimension),
                        Mode = ResizeMode.Max
                    };

                var image = new Image(input).Resize(options);

                image.Save(result);

                return (Stream)result;
            });
        }

        public Task<bool> IsValidImageAsync(Stream input)
        {
            return Task.Run(() =>
            {
                try
                {
                    var image = new Image(input);

                    return image.Width > 0 && image.Height > 0;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
