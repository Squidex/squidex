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
    private readonly IAssetThumbnailGenerator assetGenerator;

    public ImageAssetMetadataSource(IAssetThumbnailGenerator assetGenerator)
    {
        this.assetGenerator = assetGenerator;
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
                imageInfo = await assetGenerator.GetImageInfoAsync(uploadStream, mimeType, ct);
            }

            if (imageInfo != null)
            {
                var needsFix =
                    imageInfo.HasSensitiveMetadata ||
                    imageInfo.Orientation > ImageOrientation.TopLeft;

                if (command.File != null && needsFix)
                {
                    var tempFile = TempAssetFile.Create(command.File);

                    await using (var uploadStream = command.File.OpenRead())
                    {
                        await using (var tempStream = tempFile.OpenWrite())
                        {
                            await assetGenerator.FixAsync(uploadStream, mimeType, tempStream, ct);
                        }
                    }

                    await using (var tempStream = tempFile.OpenRead())
                    {
                        imageInfo = await assetGenerator.GetImageInfoAsync(tempStream, mimeType, ct) ?? imageInfo;
                    }

                    await command.File.DisposeAsync();

                    command.File = tempFile;
                }

                if (command.Type == AssetType.Unknown || needsFix)
                {
                    command.Type = AssetType.Image;

                    command.Metadata[KnownMetadataKeys.PixelWidth] = imageInfo.PixelWidth;
                    command.Metadata[KnownMetadataKeys.PixelHeight] = imageInfo.PixelHeight;
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

            var wh = command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth) + command.Metadata.GetInt32(KnownMetadataKeys.PixelWidth);

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

    public IEnumerable<string> Format(Asset asset)
    {
        if (asset.Type != AssetType.Image)
        {
            yield break;
        }

        if (asset.Metadata.TryGetNumber(KnownMetadataKeys.PixelWidth, out var w) &&
            asset.Metadata.TryGetNumber(KnownMetadataKeys.PixelHeight, out var h))
        {
            yield return $"{w}x{h}px";
        }
    }
}
