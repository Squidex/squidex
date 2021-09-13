// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Assets))]
    public sealed class AssetContentController : ApiController
    {
        private readonly IAssetFileStore assetFileStore;
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetLoader assetLoader;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetFileStore assetFileStore,
            IAssetQueryService assetQuery,
            IAssetLoader assetLoader,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetFileStore = assetFileStore;
            this.assetQuery = assetQuery;
            this.assetLoader = assetLoader;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="idOrSlug">The id or slug of the asset.</param>
        /// <param name="request">The request parameters.</param>
        /// <param name="more">Optional suffix that can be used to seo-optimize the link to the image Has not effect.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{app}/{idOrSlug}/{*more}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(0.5)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAssetContentBySlug(string app, string idOrSlug, AssetContentQueryDto request, string? more = null)
        {
            var requestContext = Context.Clone(b => b.WithoutAssetEnrichment());

            var asset = await assetQuery.FindAsync(requestContext, DomainId.Create(idOrSlug), ct: HttpContext.RequestAborted);

            if (asset == null)
            {
                asset = await assetQuery.FindBySlugAsync(requestContext, idOrSlug, HttpContext.RequestAborted);
            }

            return await DeliverAssetAsync(requestContext, asset, request);
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The request parameters.</param>
        /// <returns>
        /// 200 => Asset found and content or (resized) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{id}/")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(0.5)]
        [AllowAnonymous]
        [Obsolete("Use overload with app name")]
        public async Task<IActionResult> GetAssetContent(DomainId id, AssetContentQueryDto request)
        {
            var requestContext = Context.Clone(b => b.WithoutAssetEnrichment());

            var asset = await assetQuery.FindGlobalAsync(requestContext, id, HttpContext.RequestAborted);

            return await DeliverAssetAsync(requestContext, asset, request);
        }

        private async Task<IActionResult> DeliverAssetAsync(Context context, IAssetEntity? asset, AssetContentQueryDto request)
        {
            request ??= new AssetContentQueryDto();

            if (asset == null)
            {
                return NotFound();
            }

            if (asset.IsProtected && !Resources.CanReadAssets)
            {
                Response.Headers[HeaderNames.CacheControl] = "public,max-age=0";

                return StatusCode(403);
            }

            if (asset != null && request.Version > EtagVersion.Any && asset.Version != request.Version)
            {
                if (context.App != null)
                {
                    asset = await assetQuery.FindAsync(context, asset.Id, request.Version, HttpContext.RequestAborted);
                }
                else
                {
                    // Fallback for old endpoint. Does not set the surrogate key.
                    asset = await assetLoader.GetAsync(asset.AppId.Id, asset.Id, request.Version);
                }
            }

            if (asset == null)
            {
                return NotFound();
            }

            var resizeOptions = request.ToResizeOptions(asset);

            FileCallback callback;

            Response.Headers[HeaderNames.ETag] = asset.FileVersion.ToString(CultureInfo.InvariantCulture);

            if (request.CacheDuration > 0)
            {
                Response.Headers[HeaderNames.CacheControl] = $"public,max-age={request.CacheDuration}";
            }

            var contentLength = (long?)null;

            if (asset.Type == AssetType.Image && resizeOptions.IsValid)
            {
                callback = async (bodyStream, range, ct) =>
                {
                    if (request.ForceResize)
                    {
                        await ResizeAsync(asset, bodyStream, resizeOptions, true, ct);
                    }
                    else
                    {
                        try
                        {
                            var suffix = resizeOptions.ToString();

                            await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, suffix, bodyStream, ct: ct);
                        }
                        catch (AssetNotFoundException)
                        {
                            await ResizeAsync(asset, bodyStream, resizeOptions, false, ct);
                        }
                    }
                };
            }
            else
            {
                contentLength = asset.FileSize;

                callback = async (bodyStream, range, ct) =>
                {
                    await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, null, bodyStream, range, ct);
                };
            }

            return new FileCallbackResult(asset.MimeType, callback)
            {
                EnableRangeProcessing = contentLength > 0,
                ErrorAs404 = true,
                FileDownloadName = asset.FileName,
                FileSize = contentLength,
                LastModified = asset.LastModified.ToDateTimeOffset(),
                SendInline = request.Download != 1
            };
        }

        private async Task ResizeAsync(IAssetEntity asset, Stream bodyStream, ResizeOptions resizeOptions, bool overwrite,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("Resize"))
            {
                await using (var destinationStream = GetTempStream())
                {
                    // Do not use cancellation for the resize process because it is valuable to complete it.
                    await ResizeAsync(asset, resizeOptions, destinationStream, overwrite);

                    await destinationStream.CopyToAsync(bodyStream, ct);
                }
            }
        }

        private async Task ResizeAsync(IAssetEntity asset, ResizeOptions resizeOptions, FileStream stream,  bool overwrite)
        {
#pragma warning disable MA0040 // Flow the cancellation token
            var suffix = resizeOptions.ToString();

            await using (var sourceStream = GetTempStream())
            {
                using (Telemetry.Activities.StartActivity("ResizeDownload"))
                {
                    await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, null, sourceStream);
                    sourceStream.Position = 0;
                }

                using (Telemetry.Activities.StartActivity("ResizeImage"))
                {
                    try
                    {
                        await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, stream, resizeOptions);
                        stream.Position = 0;
                    }
                    catch
                    {
                        sourceStream.Position = 0;
                        await sourceStream.CopyToAsync(stream);
                    }
                }

                try
                {
                    using (Telemetry.Activities.StartActivity("ResizeUpload"))
                    {
                        await assetFileStore.UploadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, suffix, stream, overwrite);
                        stream.Position = 0;
                    }
                }
                catch (AssetAlreadyExistsException)
                {
                    stream.Position = 0;
                }
            }
#pragma warning restore MA0040 // Flow the cancellation token
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
