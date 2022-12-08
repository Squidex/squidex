// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppsController : ApiController
{
    private readonly IAppProvider appProvider;

    public AppsController(ICommandBus commandBus, IAppProvider appProvider)
        : base(commandBus)
    {
        this.appProvider = appProvider;
    }

    /// <summary>
    /// Get your apps.
    /// </summary>
    /// <response code="200">Apps returned.</response>.
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
        var userOrClientId = UserOrClientId!;
        var userPermissions = Resources.Context.UserPermissions;

        var apps = await appProvider.GetUserAppsAsync(userOrClientId, userPermissions, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return apps.OrderBy(x => x.Name).Select(a => AppDto.FromDomain(a, userOrClientId, IsFrontend, Resources)).ToArray();
        });

        Response.Headers[HeaderNames.ETag] = apps.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Get team apps.
    /// </summary>
    /// <param name="team">The ID of the team.</param>
    /// <response code="200">Apps returned.</response>.
    /// <remarks>
    /// You can only retrieve the list of apps when you are authenticated as a user (OpenID implicit flow).
    /// You will retrieve all apps, where you are assigned as a contributor.
    /// </remarks>
    [HttpGet]
    [Route("teams/{team}/apps")]
    [ProducesResponseType(typeof(AppDto[]), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> GetTeamApps(string team)
    {
        var apps = await appProvider.GetTeamAppsAsync(Team.Id, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return apps.OrderBy(x => x.Name).Select(a => AppDto.FromDomain(a, UserOrClientId, IsFrontend, Resources)).ToArray();
        });

        Response.Headers[HeaderNames.ETag] = apps.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Get an app by name.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Apps returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}")]
    [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(0)]
    public IActionResult GetApp(string app)
    {
        var response = Deferred.Response(() =>
        {
            var isFrontend = HttpContext.User.IsInClient(DefaultClients.Frontend);

            return AppDto.FromDomain(App, UserOrClientId, IsFrontend, Resources);
        });

        Response.Headers[HeaderNames.ETag] = App.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Create a new app.
    /// </summary>
    /// <param name="request">The app object that needs to be added to Squidex.</param>
    /// <response code="201">App created.</response>.
    /// <response code="400">App request not valid.</response>.
    /// <response code="409">App name is already in use.</response>.
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
    /// <response code="200">App updated.</response>.
    /// <response code="400">App request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut]
    [Route("apps/{app}/")]
    [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppUpdate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutApp(string app, [FromBody] UpdateAppDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return Ok(response);
    }

    /// <summary>
    /// Transfer the app.
    /// </summary>
    /// <param name="app">The name of the app to update.</param>
    /// <param name="request">The team information.</param>
    /// <response code="200">App transferred.</response>.
    /// <response code="400">App request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut]
    [Route("apps/{app}/team")]
    [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppTransfer)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutAppTeam(string app, [FromBody] TransferToTeamDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return Ok(response);
    }

    /// <summary>
    /// Upload the app image.
    /// </summary>
    /// <param name="app">The name of the app to update.</param>
    /// <param name="file">The file to upload.</param>
    /// <response code="200">App image uploaded.</response>.
    /// <response code="400">App request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/image")]
    [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppImageUpload)]
    [ApiCosts(0)]
    public async Task<IActionResult> UploadImage(string app, IFormFile file)
    {
        var response = await InvokeCommandAsync(CreateCommand(file));

        return Ok(response);
    }

    /// <summary>
    /// Remove the app image.
    /// </summary>
    /// <param name="app">The name of the app to update.</param>
    /// <response code="200">App image removed.</response>.
    /// <response code="404">App not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/image")]
    [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppImageDelete)]
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
    /// <response code="204">App deleted.</response>.
    /// <response code="404">App not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission(PermissionIds.AppDelete)]
    [ApiCosts(0)]
    public async Task<IActionResult> DeleteApp(string app)
    {
        var command = new DeleteApp();

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    private Task<AppDto> InvokeCommandAsync(ICommand command)
    {
        return InvokeCommandAsync(command, x =>
        {
            return AppDto.FromDomain(x, UserOrClientId, IsFrontend, Resources);
        });
    }

    private async Task<T> InvokeCommandAsync<T>(ICommand command, Func<IAppEntity, T> converter)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

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
}
