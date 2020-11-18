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
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Log;
using Squidex.Web;

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one

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
        /// <param name="queries">The query string parameters.</param>
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
        public async Task<IActionResult> GetAssetContentBySlug(string app, string idOrSlug, [FromQuery] AssetContentQueryDto queries, string? more = null)
        {
            var asset = await assetRepository.FindAssetAsync(AppId, DomainId.Create(idOrSlug));

            if (asset == null)
            {
                asset = await assetRepository.FindAssetBySlugAsync(AppId, idOrSlug);
            }

            if (asset != null && queries.Version > EtagVersion.Any && asset.Version != queries.Version)
            {
                asset = await assetLoader.GetAsync(App.Id, asset.Id, queries.Version);
            }

            return DeliverAsset(asset, queries);
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="id">The id of the asset.</param>
        /// <param name="queries">The query string parameters.</param>
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
        [Obsolete("Use overload with app name")]
        public async Task<IActionResult> GetAssetContent(DomainId id, [FromQuery] AssetContentQueryDto queries)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            return DeliverAsset(asset, queries);
        }

        private IActionResult DeliverAsset(IAssetEntity? asset, AssetContentQueryDto queries)
        {
            queries ??= new AssetContentQueryDto();

            if (asset == null)
            {
                return NotFound();
            }

            if (asset.IsProtected && !Resources.CanReadAssets)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age=0";

                return StatusCode(403);
            }

            var resizeOptions = queries.ToResizeOptions(asset);

            FileCallback callback;

            Response.Headers[HeaderNames.ETag] = asset.FileVersion.ToString();

            if (queries.CacheDuration > 0)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age={queries.CacheDuration}";
            }

            var contentLength = (long?)null;

            if (asset.Type == AssetType.Image && resizeOptions.IsValid)
            {
                callback = async (bodyStream, range, ct) =>
                {
                    var resizedAsset = $"{asset.AppId.Id}_{asset.Id}_{asset.FileVersion}_{resizeOptions}";

                    if (queries.ForceResize)
                    {
                        await ResizeAsync(asset, bodyStream, resizedAsset, resizeOptions, true, ct);
                    }
                    else
                    {
                        try
                        {
                            await assetStore.DownloadAsync(resizedAsset, bodyStream, ct: ct);
                        }
                        catch (AssetNotFoundException)
                        {
                            await ResizeAsync(asset, bodyStream, resizedAsset, resizeOptions, false, ct);
                        }
                    }
                };
            }
            else
            {
                contentLength = asset.FileSize;

                callback = async (bodyStream, range, ct) =>
                {
                    await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, bodyStream, range, ct);
                };
            }

            return new FileCallbackResult(asset.MimeType, callback)
            {
                EnableRangeProcessing = contentLength > 0,
                ErrorAs404 = true,
                FileDownloadName = asset.FileName,
                FileSize = contentLength,
                LastModified = asset.LastModified.ToDateTimeOffset(),
                SendInline = queries.Download != 1
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
                            await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, sourceStream);
                            sourceStream.Position = 0;
                        }

                        using (Profiler.Trace("ResizeImage"))
                        {
                            try
                            {
                                await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, resizeOptions);
                                destinationStream.Position = 0;
                            }
                            catch
                            {
                                sourceStream.Position = 0;
                                await sourceStream.CopyToAsync(destinationStream);
                            }
                        }

                        try
                        {
                            using (Profiler.Trace("ResizeUpload"))
                            {
                                await assetStore.UploadAsync(fileName, destinationStream, overwrite);
                                destinationStream.Position = 0;
                            }
                        }
                        catch (AssetAlreadyExistsException)
                        {
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