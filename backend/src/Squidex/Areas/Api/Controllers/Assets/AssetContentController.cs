// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Shared;
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
        private readonly IAssetFileStore assetFileStore;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetLoader assetLoader;
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetFileStore assetFileStore,
            IAssetRepository assetRepository,
            IAssetLoader assetLoader,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetFileStore = assetFileStore;
            this.assetRepository = assetRepository;
            this.assetLoader = assetLoader;
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="idOrSlug">The id or slug of the asset.</param>
        /// <param name="more">Optional suffix that can be used to seo-optimize the link to the image Has not effect.</param>
        /// <param name="query">The query string parameters.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{app}/{idOrSlug}/{*more}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiPermission]
        [ApiCosts(0.5)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAssetContentBySlug(string app, string idOrSlug, string more, [FromQuery] AssetContentQueryDto query)
        {
            IAssetEntity? asset;

            if (Guid.TryParse(idOrSlug, out var guid))
            {
                asset = await assetRepository.FindAssetAsync(guid);
            }
            else
            {
                asset = await assetRepository.FindAssetBySlugAsync(App.Id, idOrSlug);
            }

            return await DeliverAssetAsync(asset, query);
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="id">The id of the asset.</param>
        /// <param name="query">The query string parameters.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{id}/")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiPermission]
        [ApiCosts(0.5)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAssetContent(Guid id, [FromQuery] AssetContentQueryDto query)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            return await DeliverAssetAsync(asset, query);
        }

        private async Task<IActionResult> DeliverAssetAsync(IAssetEntity? asset, AssetContentQueryDto query)
        {
            query ??= new AssetContentQueryDto();

            if (asset == null)
            {
                return NotFound();
            }

            if (asset.IsProtected && !this.HasPermission(Permissions.AppAssetsRead))
            {
                return StatusCode(403);
            }

            if (query.Version > EtagVersion.Any && asset.Version != query.Version)
            {
                asset = await assetLoader.GetAsync(asset.Id, query.Version);
            }

            var resizeOptions = query.ToResizeOptions(asset);

            FileCallback callback;

            Response.Headers[HeaderNames.ETag] = asset.FileVersion.ToString();

            if (query.CacheDuration > 0)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age={query.CacheDuration}";
            }

            var contentLength = (long?)null;

            if (asset.Type == AssetType.Image && resizeOptions.IsValid)
            {
                callback = new FileCallback(async (bodyStream, range, ct) =>
                {
                    var resizedAsset = $"{asset.Id}_{asset.FileVersion}_{resizeOptions}";

                    if (query.ForceResize)
                    {
                        await ResizeAsync(asset, bodyStream, resizedAsset, resizeOptions, true, ct);
                    }
                    else
                    {
                        try
                        {
                            await assetStore.DownloadAsync(resizedAsset, bodyStream);
                        }
                        catch (AssetNotFoundException)
                        {
                            await ResizeAsync(asset, bodyStream, resizedAsset, resizeOptions, false, ct);
                        }
                    }
                });
            }
            else
            {
                contentLength = asset.FileSize;

                callback = new FileCallback(async (bodyStream, range, ct) =>
                {
                    await assetFileStore.DownloadAsync(asset.Id, asset.FileVersion, bodyStream, range, ct);
                });
            }

            return new FileCallbackResult(asset.MimeType, callback)
            {
                EnableRangeProcessing = contentLength > 0,
                ErrorAs404 = true,
                FileDownloadName = asset.FileName,
                FileSize = contentLength,
                LastModified = asset.LastModified.ToDateTimeOffset(),
                SendInline = query.Download != 1
            };
        }

        private async Task ResizeAsync(IAssetEntity asset, Stream bodyStream, string fileName, ResizeOptions resizeOptions, bool overwrite, CancellationToken ct)
        {
            using (Profiler.Trace("Resize"))
            {
                using (var sourceStream = GetTempStream())
                {
                    using (var destinationStream = GetTempStream())
                    {
                        using (Profiler.Trace("ResizeDownload"))
                        {
                            await assetFileStore.DownloadAsync(asset.Id, asset.FileVersion, sourceStream);
                            sourceStream.Position = 0;
                        }

                        using (Profiler.Trace("ResizeImage"))
                        {
                            await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, resizeOptions);
                            destinationStream.Position = 0;
                        }

                        using (Profiler.Trace("ResizeUpload"))
                        {
                            await assetStore.UploadAsync(fileName, destinationStream, overwrite);
                            destinationStream.Position = 0;
                        }

                        await destinationStream.CopyToAsync(bodyStream, ct);
                    }
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