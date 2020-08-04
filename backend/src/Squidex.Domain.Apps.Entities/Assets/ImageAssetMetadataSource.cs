﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class ImageAssetMetadataSource : IAssetMetadataSource
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageAssetMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

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

        public async Task EnhanceAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            if (command.Type == AssetType.Unknown || command.Type == AssetType.Image)
            {
                ImageInfo? imageInfo = null;

                using (var uploadStream = command.File.OpenRead())
                {
                    imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);
                }

                if (imageInfo != null)
                {
                    var isSwapped = imageInfo.IsRotatedOrSwapped;

                    if (isSwapped)
                    {
                        var tempFile = new TempAssetFile(command.File);

                        using (var uploadStream = command.File.OpenRead())
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

            if (command.Type == AssetType.Image && tags != null)
            {
                tags.Add("image");

                var wh = command.Metadata.GetPixelWidth() + command.Metadata.GetPixelWidth();

                if (wh > 2000)
                {
                    tags.Add("image/large");
                }
                else if (wh > 1000)
                {
                    tags.Add("image/medium");
                }
                else
                {
                    tags.Add("image/small");
                }
            }
        }

        public IEnumerable<string> Format(IAssetEntity asset)
        {
            if (asset.Type == AssetType.Image)
            {
                if (asset.Metadata.TryGetNumber("pixelWidth", out var w) &&
                    asset.Metadata.TryGetNumber("pixelHeight", out var h))
                {
                    yield return $"{w}x{h}px";
                }
            }
        }
    }
}
