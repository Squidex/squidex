// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Web;

#pragma warning disable 1573

namespace Squidex.Areas.Api.Controllers.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Assets))]
    public sealed class AssetContentController : ApiController
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetStore assetStore,
            IAssetRepository assetRepository,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetStore = assetStore;
            this.assetRepository = assetRepository;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="idOrSlug">The id or slug of the asset.</param>
        /// <param name="more">Optional suffix that can be used to seo-optimize the link to the image Has not effect.</param>
        /// <param name="version">The optional version of the asset.</param>
        /// <param name="dl">Set it to 0 to prevent download.</param>
        /// <param name="width">The target width of the asset, if it is an image.</param>
        /// <param name="height">The target height of the asset, if it is an image.</param>
        /// <param name="quality">Optional image quality, it is is an jpeg image.</param>
        /// <param name="mode">The resize mode when the width and height is defined.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{app}/{idOrSlug}/{*more}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiCosts(0.5)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAssetContentBySlug(string app, string idOrSlug, string more,
            [FromQuery] long version = EtagVersion.Any,
            [FromQuery] int dl = 1,
            [FromQuery] int? width = null,
            [FromQuery] int? height = null,
            [FromQuery] int? quality = null,
            [FromQuery] string mode = null)
        {
            IAssetEntity asset;

            if (Guid.TryParse(idOrSlug, out var guid))
            {
                asset = await assetRepository.FindAssetAsync(guid);
            }
            else
            {
                asset = await assetRepository.FindAssetBySlugAsync(App.Id, idOrSlug);
            }

            return DeliverAsset(asset, version, width, height, quality, mode, dl);
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="id">The id of the asset.</param>
        /// <param name="more">Optional suffix that can be used to seo-optimize the link to the image Has not effect.</param>
        /// <param name="version">The optional version of the asset.</param>
        /// <param name="dl">Set it to 0 to prevent download.</param>
        /// <param name="width">The target width of the asset, if it is an image.</param>
        /// <param name="height">The target height of the asset, if it is an image.</param>
        /// <param name="quality">Optional image quality, it is is an jpeg image.</param>
        /// <param name="mode">The resize mode when the width and height is defined.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{id}/{*more}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiCosts(0.5)]
        public async Task<IActionResult> GetAssetContent(Guid id, string more,
            [FromQuery] long version = EtagVersion.Any,
            [FromQuery] int dl = 1,
            [FromQuery] int? width = null,
            [FromQuery] int? height = null,
            [FromQuery] int? quality = null,
            [FromQuery] string mode = null)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            return DeliverAsset(asset, version, width, height, quality, mode, dl);
        }

        private IActionResult DeliverAsset(IAssetEntity asset, long version, int? width, int? height, int? quality, string mode, int download = 1)
        {
            if (asset == null || asset.FileVersion < version || width == 0 || height == 0 || quality == 0)
            {
                return NotFound();
            }

            Response.Headers[HeaderNames.ETag] = asset.FileVersion.ToString();

            var handler = new Func<Stream, Task>(async bodyStream =>
            {
                var assetId = asset.Id.ToString();

                if (asset.IsImage && (width.HasValue || height.HasValue || quality.HasValue))
                {
                    var assetSuffix = $"{width}_{height}_{mode}";

                    if (quality.HasValue)
                    {
                        assetSuffix += $"_{quality}";
                    }

                    try
                    {
                        await assetStore.DownloadAsync(assetId, asset.FileVersion, assetSuffix, bodyStream);
                    }
                    catch (AssetNotFoundException)
                    {
                        using (Profiler.Trace("Resize"))
                        {
                            using (var sourceStream = GetTempStream())
                            {
                                using (var destinationStream = GetTempStream())
                                {
                                    using (Profiler.Trace("ResizeDownload"))
                                    {
                                        await assetStore.DownloadAsync(assetId, asset.FileVersion, null, sourceStream);
                                        sourceStream.Position = 0;
                                    }

                                    using (Profiler.Trace("ResizeImage"))
                                    {
                                        await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, width, height, mode, quality);
                                        destinationStream.Position = 0;
                                    }

                                    using (Profiler.Trace("ResizeUpload"))
                                    {
                                        await assetStore.UploadAsync(assetId, asset.FileVersion, assetSuffix, destinationStream);
                                        destinationStream.Position = 0;
                                    }

                                    await destinationStream.CopyToAsync(bodyStream);
                                }
                            }
                        }
                    }
                }
                else
                {
                    await assetStore.DownloadAsync(assetId, asset.FileVersion, null, bodyStream);
                }
            });

            if (download == 1)
            {
                return new FileCallbackResult(asset.MimeType, asset.FileName, true, handler);
            }
            else
            {
                return new FileCallbackResult(asset.MimeType, null, true, handler);
            }
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