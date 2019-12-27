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
    public sealed class ImageMetadataSource : IAssetMetadataSource
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageMetadataSource(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(assetThumbnailGenerator);

            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        public async Task EnhanceAsync(UploadAssetCommand command, HashSet<string>? tags)
        {
            var imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());

            if (imageInfo != null)
            {
                command.Type = AssetType.Image;

                command.Metadata.SetPixelWidth(imageInfo.PixelWidth);
                command.Metadata.SetPixelHeight(imageInfo.PixelHeight);

                if (tags != null)
                {
                    tags.Add("image");

                    var wh = imageInfo.PixelWidth + imageInfo.PixelHeight;

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
        }
    }
}
