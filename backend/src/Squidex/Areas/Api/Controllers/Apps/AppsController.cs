// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppsController : ApiController
    {
        private static readonly ResizeOptions ResizeOptions = new ResizeOptions { Width = 50, Height = 50, Mode = ResizeMode.Crop };
        private readonly IAppImageStore appImageStore;
        private readonly IAppProvider appProvider;
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AppsController(ICommandBus commandBus,
            IAppImageStore appImageStore,
            IAppProvider appProvider,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.appImageStore = appImageStore;
            this.appProvider = appProvider;
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get your apps.
        /// </summary>
        /// <returns>
        /// 200 => Apps returned.
        /// </returns>
        /// <remarks>
        /// You can only retrieve the list of apps when you are authenticated as a user (OpenID implicit flow).
        /// You will retrieve all apps, where you are assigned as a contributor.
        /// </remarks>
        [HttpGet]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppDto[]), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(0)]
        public async Task<IActionResult> GetApps()
        {
            var userOrClientId = HttpContext.User.UserOrClientId()!;
            var userPermissions = Resources.Context.UserPermissions;

            var apps = await appProvider.GetUserAppsAsync(userOrClientId, userPermissions, HttpContext.RequestAborted);

            var response = Deferred.Response(() =>
            {
                var isFrontend = HttpContext.User.IsInClient(DefaultClients.Frontend);

                return apps.OrderBy(x => x.Name).Select(a => AppDto.FromApp(a, userOrClientId, isFrontend, Resources)).ToArray();
            });

            Response.Headers[HeaderNames.ETag] = apps.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Get an app by name.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Apps returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(0)]
        public IActionResult GetApp(string app)
        {
            var response = Deferred.Response(() =>
            {
                var userOrClientId = HttpContext.User.UserOrClientId()!;

                var isFrontend = HttpContext.User.IsInClient(DefaultClients.Frontend);

                return AppDto.FromApp(App, userOrClientId, isFrontend, Resources);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create a new app.
        /// </summary>
        /// <param name="request">The app object that needs to be added to Squidex.</param>
        /// <returns>
        /// 201 => App created.
        /// 400 => App request not valid.
        /// 409 => App name is already in use.
        /// </returns>
        /// <remarks>
        /// You can only create an app when you are authenticated as a user (OpenID implicit flow).
        /// You will be assigned as owner of the new app automatically.
        /// </remarks>
        [HttpPost]
        [Route("apps/")]
        [ProducesResponseType(typeof(AppDto), 201)]
        [ApiPermission]
        [ApiCosts(0)]
        public async Task<IActionResult> PostApp([FromBody] CreateAppDto request)
        {
            var response = await InvokeCommandAsync(request.ToCommand());

            return CreatedAtAction(nameof(GetApps), response);
        }

        /// <summary>
        /// Update the app.
        /// </summary>
        /// <param name="app">The name of the app to update.</param>
        /// <param name="request">The values to update.</param>
        /// <returns>
        /// 200 => App updated.
        /// 400 => App request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppUpdate)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutApp(string app, [FromBody] UpdateAppDto request)
        {
            var response = await InvokeCommandAsync(request.ToCommand());

            return Ok(response);
        }

        /// <summary>
        /// Upload the app image.
        /// </summary>
        /// <param name="app">The name of the app to update.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 200 => App image uploaded.
        /// 400 => App request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/image")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppImageUpload)]
        [ApiCosts(0)]
        public async Task<IActionResult> UploadImage(string app, IFormFile file)
        {
            var response = await InvokeCommandAsync(CreateCommand(file));

            return Ok(response);
        }

        /// <summary>
        /// Get the app image.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => App image found and content or (resized) image returned.
        /// 404 => App not found.
        /// </returns>
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
                    using (Telemetry.Activities.StartActivity("Resize"))
                    {
                        await using (var destinationStream = GetTempStream())
                        {
                            await ResizeAsync(resizedAsset, destinationStream);

                            await destinationStream.CopyToAsync(body, ct);
                        }
                    }
                }
            });

            return new FileCallbackResult(App.Image.MimeType, callback)
            {
                ErrorAs404 = true
            };
        }

        private async Task ResizeAsync(string resizedAsset, FileStream destinationStream)
        {
#pragma warning disable MA0040 // Flow the cancellation token
            await using (var sourceStream = GetTempStream())
            {
                using (Telemetry.Activities.StartActivity("ResizeDownload"))
                {
                    await appImageStore.DownloadAsync(App.Id, sourceStream);
                    sourceStream.Position = 0;
                }

                using (Telemetry.Activities.StartActivity("ResizeImage"))
                {
                    await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, ResizeOptions);
                    destinationStream.Position = 0;
                }

                using (Telemetry.Activities.StartActivity("ResizeUpload"))
                {
                    await assetStore.UploadAsync(resizedAsset, destinationStream);
                    destinationStream.Position = 0;
                }
            }
#pragma warning restore MA0040 // Flow the cancellation token
        }

        /// <summary>
        /// Remove the app image.
        /// </summary>
        /// <param name="app">The name of the app to update.</param>
        /// <returns>
        /// 200 => App image removed.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/image")]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppImageDelete)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteImage(string app)
        {
            var response = await InvokeCommandAsync(new RemoveAppImage());

            return Ok(response);
        }

        /// <summary>
        /// Delete the app.
        /// </summary>
        /// <param name="app">The name of the app to delete.</param>
        /// <returns>
        /// 204 => App deleted.
        /// 404 => App not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/")]
        [ApiPermission(Permissions.AppDelete)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteApp(string app)
        {
            await CommandBus.PublishAsync(new DeleteApp());

            return NoContent();
        }

        private Task<AppDto> InvokeCommandAsync(ICommand command)
        {
            return InvokeCommandAsync(command, x =>
            {
                var userOrClientId = HttpContext.User.UserOrClientId()!;

                var isFrontend = HttpContext.User.IsInClient(DefaultClients.Frontend);

                return AppDto.FromApp(x, userOrClientId, isFrontend, Resources);
            });
        }

        private async Task<T> InvokeCommandAsync<T>(ICommand command, Func<IAppEntity, T> converter)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = converter(result);

            return response;
        }

        private UploadAppImage CreateCommand(IFormFile? file)
        {
            if (file == null || Request.Form.Files.Count != 1)
            {
                var error = T.Get("validation.onlyOneFile");

                throw new ValidationException(error);
            }

            return new UploadAppImage { File = file.ToAssetFile() };
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
