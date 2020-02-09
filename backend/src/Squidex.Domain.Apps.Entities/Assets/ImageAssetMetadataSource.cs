// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
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
            Guard.NotNull(assetThumbnailGenerator);

            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        public async Task EnhanceAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            if (command.Type == AssetType.Unknown)
            {
                using (var uploadStream = command.File.OpenRead())
                {
                    var imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);

                    if (imageInfo != null)
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
