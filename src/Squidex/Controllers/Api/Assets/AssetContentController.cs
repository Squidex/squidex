// ==========================================================================
//  AssetContentController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;
using Squidex.Read.Assets.Repositories;

#pragma warning disable 1573

namespace Squidex.Controllers.Api.Assets
{
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerIgnore]
    public class AssetContentController : ControllerBase
    {
        private readonly IAssetStore assetStorage;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetStore assetStorage,
            IAssetRepository assetRepository,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetRepository = assetRepository;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("assets/{id}/")]
        public async Task<IActionResult> GetAssetContent(string app, Guid id, [FromQuery] int? width = null, [FromQuery] int? height = null, [FromQuery] string mode = null)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            return new FileCallbackResult(asset.MimeType, asset.FileName, async bodyStream =>
            {
                if (asset.IsImage && (width.HasValue || height.HasValue))
                {
                    var suffix = $"{width}_{height}_{mode}";

                    try
                    {
                        await assetStorage.DownloadAsync(asset.Id, asset.Version, suffix, bodyStream);
                    }
                    catch (AssetNotFoundException)
                    {
                        using (var tempStream1 = GetTempStream())
                        {
                            using (var tempStream2 = GetTempStream())
                            {
                                await assetStorage.DownloadAsync(asset.Id, asset.Version, null, tempStream1);
                                tempStream1.Position = 0;

                                await assetThumbnailGenerator.CreateThumbnailAsync(tempStream1, tempStream2, width, height, mode);
                                tempStream2.Position = 0;

                                await assetStorage.UploadAsync(asset.Id, asset.Version, suffix, tempStream2);
                                tempStream2.Position = 0;

                                await tempStream2.CopyToAsync(bodyStream);
                            }
                        }

                    }
                }

                await assetStorage.DownloadAsync(asset.Id, asset.Version, null, bodyStream);
            });
        }

        private static FileStream GetTempStream()
        {
            var tempFileName = Path.GetTempFileName();

            return new FileStream(tempFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Delete, 1024 * 16,
                FileOptions.Asynchronous |
                FileOptions.DeleteOnClose |
                FileOptions.SequentialScan);
        }
    }
}

#pragma warning restore 1573