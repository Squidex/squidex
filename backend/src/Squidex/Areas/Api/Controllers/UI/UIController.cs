// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers.UI.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.UI;

public sealed class UIController : ApiController
{
    private static readonly DomainId SharedKey = DomainId.Create("__shared");
    private static readonly Permission CreateAppPermission = new Permission(PermissionIds.AdminAppCreate);
    private static readonly Permission CreateTeamPermission = new Permission(PermissionIds.AdminTeamCreate);
    private readonly MyUIOptions uiOptions;
    private readonly IAppUISettings appUISettings;

    public UIController(ICommandBus commandBus, IOptions<MyUIOptions> uiOptions, IAppUISettings appUISettings)
        : base(commandBus)
    {
        this.uiOptions = uiOptions.Value;
        this.appUISettings = appUISettings;
    }

    /// <summary>
    /// Get ui settings.
    /// </summary>
    /// <response code="200">UI settings returned.</response>
    [HttpGet]
    [Route("ui/settings/")]
    [ProducesResponseType(typeof(Dictionary<string, JsonValue>), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetCommonSettings()
    {
        var common = await appUISettings.GetAsync(SharedKey, UserId, HttpContext.RequestAborted);

        var result = new Dictionary<string, JsonValue>
        {
            ["canCreateApps"] = !uiOptions.OnlyAdminsCanCreateApps || Context.UserPermissions.Includes(CreateAppPermission),
            ["canCreateTeams"] = !uiOptions.OnlyAdminsCanCreateTeams || Context.UserPermissions.Includes(CreateTeamPermission),
        };

        foreach (var (key, value) in common)
        {
            result[key] = value;
        }

        return Ok(result);
    }

    /// <summary>
    /// Get ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">UI settings returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/ui/settings/")]
    [ProducesResponseType(typeof(Dictionary<string, JsonValue>), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetSettings(string app)
    {
        var result = await appUISettings.GetAsync(AppId, null, HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Get my ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">UI settings returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/ui/settings/me")]
    [ProducesResponseType(typeof(Dictionary<string, JsonValue>), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetUserSettings(string app)
    {
        var result = await appUISettings.GetAsync(AppId, UserId, HttpContext.RequestAborted);

        return Ok(result);
    }

    /// <summary>
    /// Set ui settings.
    /// </summary>
    /// <param name="key">The name of the setting.</param>
    /// <param name="request">The request with the value to update.</param>
    /// <response code="200">UI setting set.</response>
    /// <response code="404">App not found.</response>
    [HttpPut]
    [Route("ui/settings/{key}")]
    [ApiPermission]
    public async Task<IActionResult> PutCommonSetting(string key, [FromBody] UpdateSettingDto request)
    {
        await appUISettings.SetAsync(SharedKey, UserId, key, request.Value, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Set ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="key">The name of the setting.</param>
    /// <param name="request">The request with the value to update.</param>
    /// <response code="200">UI setting set.</response>
    /// <response code="404">App not found.</response>
    [HttpPut]
    [Route("apps/{app}/ui/settings/{key}")]
    [ApiPermission]
    public async Task<IActionResult> PutSetting(string app, string key, [FromBody] UpdateSettingDto request)
    {
        await appUISettings.SetAsync(AppId, null, key, request.Value, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Set my ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="key">The name of the setting.</param>
    /// <param name="request">The request with the value to update.</param>
    /// <response code="200">UI setting set.</response>
    /// <response code="404">App not found.</response>
    [HttpPut]
    [Route("apps/{app}/ui/settings/me/{key}")]
    [ApiPermission]
    public async Task<IActionResult> PutUserSetting(string app, string key, [FromBody] UpdateSettingDto request)
    {
        await appUISettings.SetAsync(AppId, UserId, key, request.Value, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove ui settings.
    /// </summary>
    /// <param name="key">The name of the setting.</param>
    /// <response code="200">UI setting removed.</response>
    /// <response code="404">App not found.</response>
    [HttpDelete]
    [Route("ui/settings/{key}")]
    [ApiPermission]
    public async Task<IActionResult> DeleteCommonSetting(string key)
    {
        await appUISettings.RemoveAsync(SharedKey, UserId, key, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="key">The name of the setting.</param>
    /// <response code="200">UI setting removed.</response>
    /// <response code="404">App not found.</response>
    [HttpDelete]
    [Route("apps/{app}/ui/settings/{key}")]
    [ApiPermission]
    public async Task<IActionResult> DeleteSetting(string app, string key)
    {
        await appUISettings.RemoveAsync(AppId, null, key, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Remove my ui settings.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="key">The name of the setting.</param>
    /// <response code="200">UI setting removed.</response>
    /// <response code="404">App not found.</response>
    [HttpDelete]
    [Route("apps/{app}/ui/settings/me/{key}")]
    [ApiPermission]
    public async Task<IActionResult> DeleteUserSetting(string app, string key)
    {
        await appUISettings.RemoveAsync(AppId, UserId, key, HttpContext.RequestAborted);

        return NoContent();
    }
}
