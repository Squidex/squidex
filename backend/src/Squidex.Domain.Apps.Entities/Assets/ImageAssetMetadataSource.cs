// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class ImageAssetMetadataSource : IAssetMetadataSource
{
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

    public ImageAssetMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
    {
        this.assetThumbnailGenerator = assetThumbnailGenerator;
    }

    public async Task EnhanceAsync(UploadAssetCommand command,
        CancellationToken ct)
    {
        if (command.Type is AssetType.Unknown or AssetType.Image)
        {
            var mimeType = command.File.MimeType;

            ImageInfo? imageInfo;

            await using (var uploadStream = command.File.OpenRead())
            {
                imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream, mimeType, ct);
            }

            if (imageInfo != null)
            {
                var isSwapped = imageInfo.Orientation > ImageOrientation.TopLeft;

                if (command.File != null && isSwapped)
                {
                    var tempFile = TempAssetFile.Create(command.File);

                    await using (var uploadStream = command.File.OpenRead())
                    {
                        await using (var tempStream = tempFile.OpenWrite())
                        {
                            await assetThumbnailGenerator.FixOrientationAsync(uploadStream, mimeType, tempStream, ct);
                        }
                    }

                    await using (var tempStream = tempFile.OpenRead())
                    {
                        imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(tempStream, mimeType, ct) ?? imageInfo;
                    }

                    await command.File.DisposeAsync();

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
