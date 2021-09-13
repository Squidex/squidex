// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class ImageAssetMetadataSource : IAssetMetadataSource
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageAssetMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        private sealed class TempAssetFile : AssetFile, IDisposable
        {
            public Stream Stream { get; }

            public TempAssetFile(AssetFile source)
                : base(source.FileName, source.MimeType, source.FileSize)
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                var tempStream = new FileStream(tempPath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None, 4096,
                    FileOptions.DeleteOnClose);

                Stream = tempStream;
            }

            public override void Dispose()
            {
                Stream.Dispose();
            }

            public override Stream OpenRead()
            {
                Stream.Position = 0;

                return Stream;
            }
        }

        public async Task EnhanceAsync(UploadAssetCommand command)
        {
            if (command.Type == AssetType.Unknown || command.Type == AssetType.Image)
            {
                ImageInfo? imageInfo = null;

                await using (var uploadStream = command.File.OpenRead())
                {
                    imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);
                }

                if (imageInfo != null)
                {
                    var isSwapped = imageInfo.IsRotatedOrSwapped;

                    if (isSwapped)
                    {
                        var tempFile = new TempAssetFile(command.File);

                        await using (var uploadStream = command.File.OpenRead())
                        {
                            imageInfo = await assetThumbnailGenerator.FixOrientationAsync(uploadStream, tempFile.Stream);
                        }

                        command.File.Dispose();
                        command.File = tempFile;
                    }

                    if (command.Type == AssetType.Unknown || isSwapped)
                    {
                        command.Type = AssetType.Image;

                        command.Metadata.SetPixelWidth(imageInfo.PixelWidth);
                        command.Metadata.SetPixelHeight(imageInfo.PixelHeight);
                    }
                }
            }

            if (command.Tags == null)
            {
                return;
            }

            if (command.Type == AssetType.Image)
            {
                command.Tags.Add("image");

                var wh = command.Metadata.GetPixelWidth() + command.Metadata.GetPixelWidth();

                if (wh > 2000)
                {
                    command.Tags.Add("image/large");
                }
                else if (wh > 1000)
                {
                    command.Tags.Add("image/medium");
                }
                else
                {
                    command.Tags.Add("image/small");
                }
            }
        }

        public IEnumerable<string> Format(IAssetEntity asset)
        {
            if (asset.Type == AssetType.Image)
            {
                var w = asset.Metadata.GetPixelWidth();
                var h = asset.Metadata.GetPixelHeight();

                if (w != null && h != null)
                {
                    yield return $"{w}x{h}px";
                }
            }
        }
    }
}
