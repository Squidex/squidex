// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Authorization;
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

namespace Squidex.Areas.Api.Controllers.Assets;

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
    /// <response code="200">Asset found and content or (resized) image returned.</response>.
    /// <response code="404">Asset or app not found.</response>.
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
    /// <param name="id">The ID of the asset.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="200">Asset found and content or (resized) image returned.</response>.
    /// <response code="404">Asset or app not found.</response>.
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
                asset = await assetLoader.GetAsync(asset.AppId.Id, asset.Id, request.Version, HttpContext.RequestAborted);
            }
        }

        if (asset == null)
        {
            return NotFound();
        }

        Response.Headers[HeaderNames.ETag] = asset.FileVersion.ToString(CultureInfo.InvariantCulture);

        if (request.CacheDuration > 0)
        {
            Response.Headers[HeaderNames.CacheControl] = $"public,max-age={request.CacheDuration}";
        }

        var resizeOptions = request.ToResizeOptions(asset, assetThumbnailGenerator, HttpContext.Request);

        var contentLength = (long?)null;
        var contentCallback = (FileCallback?)null;
        var contentType = asset.MimeType;

        if (asset.Type == AssetType.Image && assetThumbnailGenerator.IsResizable(asset.MimeType, resizeOptions, out var destinationMimeType))
        {
            contentType = destinationMimeType!;

            contentCallback = async (body, range, ct) =>
            {
                var suffix = resizeOptions.ToString();

                if (request.Force)
                {
                    using (Telemetry.Activities.StartActivity("Resize"))
                    {
                        await ResizeAsync(asset, suffix, body, resizeOptions, true, ct);
                    }
                }
                else
                {
                    try
                    {
                        await DownloadAsync(asset, body, suffix, range, ct);
                    }
                    catch (AssetNotFoundException)
                    {
                        await ResizeAsync(asset, suffix, body, resizeOptions, false, ct);
                    }
                }
            };
        }
        else
        {
            contentLength = asset.FileSize;

            contentCallback = async (body, range, ct) =>
            {
                await DownloadAsync(asset, body, null, range, ct);
            };
        }

        return new FileCallbackResult(contentType, contentCallback)
        {
            EnableRangeProcessing = contentLength > 0,
            ErrorAs404 = true,
            FileDownloadName = asset.FileName,
            FileSize = contentLength,
            LastModified = asset.LastModified.ToDateTimeOffset(),
            SendInline = request.Download != 1
        };
    }

    private async Task DownloadAsync(IAssetEntity asset, Stream bodyStream, string? suffix, BytesRange range,
        CancellationToken ct)
    {
        await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, suffix, bodyStream, range, ct);
    }

    private async Task ResizeAsync(IAssetEntity asset, string suffix, Stream target, ResizeOptions resizeOptions, bool overwrite,
        CancellationToken ct)
    {
#pragma warning disable MA0040 // Flow the cancellation token
        using var activity = Telemetry.Activities.StartActivity("Resize");

        await using var assetOriginal = new TempAssetFile(asset.FileName, asset.MimeType, 0);
        await using var assetResized = new TempAssetFile(asset.FileName, asset.MimeType, 0);

        using (Telemetry.Activities.StartActivity("Read"))
        {
            await using (var originalStream = assetOriginal.OpenWrite())
            {
                await assetFileStore.DownloadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, null, originalStream);
            }
        }

        using (Telemetry.Activities.StartActivity("Resize"))
        {
            try
            {
                await using (var originalStream = assetOriginal.OpenRead())
                {
                    await using (var resizeStream = assetResized.OpenWrite())
                    {
                        await assetThumbnailGenerator.CreateThumbnailAsync(originalStream, asset.MimeType, resizeStream, resizeOptions);
                    }
                }
            }
            catch
            {
                await using (var originalStream = assetOriginal.OpenRead())
                {
                    await using (var resizeStream = assetResized.OpenWrite())
                    {
                        await originalStream.CopyToAsync(resizeStream);
                    }
                }
            }
        }

        using (Telemetry.Activities.StartActivity("Save"))
        {
            try
            {
                await using (var resizeStream = assetResized.OpenRead())
                {
                    await assetFileStore.UploadAsync(asset.AppId.Id, asset.Id, asset.FileVersion, suffix, resizeStream, overwrite);
                }
            }
            catch (AssetAlreadyExistsException)
            {
                return;
            }
        }

        using (Telemetry.Activities.StartActivity("Write"))
        {
            await using (var resizeStream = assetResized.OpenRead())
            {
                await resizeStream.CopyToAsync(target, ct);
            }
        }
#pragma warning restore MA0040 // Flow the cancellation token
    }
}
