// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Teams.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams;

/// <summary>
/// Update and query teams.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Teams))]
public sealed class TeamsController : ApiController
{
    private readonly IAppProvider appProvider;

    public TeamsController(ICommandBus commandBus, IAppProvider appProvider)
        : base(commandBus)
    {
        this.appProvider = appProvider;
    }

    /// <summary>
    /// Get your teams.
    /// </summary>
    /// <response code="200">Teams returned.</response>.
    /// <remarks>
    /// You can only retrieve the list of teams when you are authenticated as a user (OpenID implicit flow).
    /// You will retrieve all teams, where you are assigned as a contributor.
    /// </remarks>
    [HttpGet]
    [Route("teams/")]
    [ProducesResponseType(typeof(TeamDto[]), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await appProvider.GetUserTeamsAsync(UserOrClientId, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return teams.OrderBy(x => x.Name).Select(a => TeamDto.FromDomain(a, UserOrClientId, Resources)).ToArray();
        });

        Response.Headers[HeaderNames.ETag] = teams.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Get an team by name.
    /// </summary>
    /// <param name="team">The name of the team.</param>
    /// <response code="200">Teams returned.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpGet]
    [Route("teams/{team}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(0)]
    public IActionResult GetTeam(string team)
    {
        var response = Deferred.Response(() =>
        {
            return TeamDto.FromDomain(Team, UserOrClientId, Resources);
        });

        Response.Headers[HeaderNames.ETag] = Team.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Create a new team.
    /// </summary>
    /// <param name="request">The team object that needs to be added to Squidex.</param>
    /// <response code="201">Team created.</response>.
    /// <response code="400">Team request not valid.</response>.
    /// <response code="409">Team name is already in use.</response>.
    /// <remarks>
    /// You can only create an team when you are authenticated as a user (OpenID implicit flow).
    /// You will be assigned as owner of the new team automatically.
    /// </remarks>
    [HttpPost]
    [Route("teams/")]
    [ProducesResponseType(typeof(TeamDto), 201)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> PostTeam([FromBody] CreateTeamDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return CreatedAtAction(nameof(GetTeams), response);
    }

    /// <summary>
    /// Update the team.
    /// </summary>
    /// <param name="team">The name of the team to update.</param>
    /// <param name="request">The values to update.</param>
    /// <response code="200">Team updated.</response>.
    /// <response code="400">Team request not valid.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpPut]
    [Route("teams/{team}/")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamUpdate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutTeam(string team, [FromBody] UpdateTeamDto request)
    {
        var response = await InvokeCommandAsync(request.ToCommand());

        return Ok(response);
    }

    private Task<TeamDto> InvokeCommandAsync(ICommand command)
    {
        return InvokeCommandAsync(command, x =>
        {
            return TeamDto.FromDomain(x, UserOrClientId, Resources);
        });
    }

    private async Task<T> InvokeCommandAsync<T>(ICommand command, Func<ITeamEntity, T> converter)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<ITeamEntity>();
        var response = converter(result);

        return response;
    }
}
