// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.ScriptLogs.Models;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.ScriptLogs;

/// <summary>
/// Readonly API to get the console output of scripts.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(ScriptLogs))]
public sealed class ScriptLogsController(ICommandBus commandBus, IScriptLogStore scriptLogStore) : ApiController(commandBus)
{
    /// <summary>
    /// Get the script logs.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="name">The optional name or name prefix of the script, for example 'contents/my-schema'.</param>
    /// <param name="skip">The number of logs to skip.</param>
    /// <param name="take">The number of logs to return.</param>
    /// <response code="200">Script logs returned.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// Returns the console output of content and asset scripts, newest first. Only the latest logs are stored.
    /// </remarks>
    [HttpGet]
    [Route("apps/{app}/script-logs/")]
    [ProducesResponseType(typeof(ScriptLogsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppScriptLogsRead)]
    [ApiCosts(0.1)]
    public async Task<IActionResult> GetScriptLogs(string app, [FromQuery] string? name = null, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var logs = await scriptLogStore.QueryAsync(AppId, name, skip, take, HttpContext.RequestAborted);

        var response = ScriptLogsDto.FromDomain(logs);

        return Ok(response);
    }
}
