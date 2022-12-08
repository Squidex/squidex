// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppImageController : ApiController
{
    private readonly IAppImageStore appImageStore;
    private readonly IAssetStore assetStore;
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

    public AppImageController(ICommandBus commandBus,
        IAppImageStore appImageStore,
        IAssetStore assetStore,
        IAssetThumbnailGenerator assetThumbnailGenerator)
        : base(commandBus)
    {
        this.appImageStore = appImageStore;
        this.assetStore = assetStore;
        this.assetThumbnailGenerator = assetThumbnailGenerator;
    }

    /// <summary>
    /// Get the app image.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">App image found and content or (resized) image returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/image")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ApiCosts(0)]
    public IActionResult GetImage(string app)
    {
        if (App.Image == null)
        {
            return NotFound();
        }

        var etag = App.Image.Etag;

        Response.Headers[HeaderNames.ETag] = etag;

        var callback = new FileCallback(async (body, range, ct) =>
        {
            var resizedAsset = $"{App.Id}_{etag}_Resized";

            try
            {
                await assetStore.DownloadAsync(resizedAsset, body, ct: ct);
            }
            catch (AssetNotFoundException)
            {
                await ResizeAsync(resizedAsset, App.Image.MimeType, body, ct);
            }
        });

        return new FileCallbackResult(App.Image.MimeType, callback)
        {
            ErrorAs404 = true
        };
    }

    private async Task ResizeAsync(string resizedAsset, string mimeType, Stream target,
        CancellationToken ct)
    {
#pragma warning disable MA0040 // Flow the cancellation token
        using var activity = Telemetry.Activities.StartActivity("Resize");

        await using var assetOriginal = new TempAssetFile(resizedAsset, mimeType, 0);
        await using var assetResized = new TempAssetFile(resizedAsset, mimeType, 0);

        var resizeOptions = new ResizeOptions
        {
            TargetWidth = 50,
            TargetHeight = 50
        };

        using (Telemetry.Activities.StartActivity("Read"))
        {
            await using (var originalStream = assetOriginal.OpenWrite())
            {
                await appImageStore.DownloadAsync(App.Id, originalStream, ct);
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
                        await assetThumbnailGenerator.CreateThumbnailAsync(originalStream, mimeType, resizeStream, resizeOptions, ct);
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
                    await assetStore.UploadAsync(resizedAsset, resizeStream);
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
