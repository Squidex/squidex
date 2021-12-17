// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Images.Models;
using Squidex.Assets;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Images.Service
{
    public sealed class InProcessImageResizer : IImageResizer
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public InProcessImageResizer(
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        public async Task<string> ResizeAsync(ResizeRequest request, CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("Resize"))
            {
                await using (var destinationStream = GetTempStream())
                {
                    await ResizeAsync(request.SourcePath, request.SourceMimeType, request.TargetPath, request.ResizeOptions, request.Overwrite);
                }
            }

            return request.TargetPath;
        }

        private async Task ResizeAsync(string sourcePath, string sourceMimeType, string targetPath, ResizeOptions resizeOptions, bool overwrite)
        {
            await using (var resizedTemp = GetTempStream())
            {
                await using (var originalTemp = GetTempStream())
                {
                    using (Telemetry.Activities.StartActivity("ResizeDownload"))
                    {
                        await assetStore.DownloadAsync(sourcePath, originalTemp);
                        originalTemp.Position = 0;
                    }

                    using (Telemetry.Activities.StartActivity("ResizeImage"))
                    {
                        await assetThumbnailGenerator.CreateThumbnailAsync(originalTemp, sourceMimeType, resizedTemp, resizeOptions);
                        resizedTemp.Position = 0;
                    }
                }

                try
                {
                    using (Telemetry.Activities.StartActivity("ResizeUpload"))
                    {
                        await assetStore.UploadAsync(targetPath, resizedTemp, overwrite);
                    }
                }
                catch (AssetAlreadyExistsException)
                {
                }
            }
        }

        private static FileStream GetTempStream()
        {
            var tempFileName = Path.GetTempFileName();

            const int bufferSize = 16 * 1024;

            return new FileStream(tempFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Delete,
                bufferSize,
                FileOptions.Asynchronous |
                FileOptions.DeleteOnClose |
                FileOptions.SequentialScan);
        }
    }
}
