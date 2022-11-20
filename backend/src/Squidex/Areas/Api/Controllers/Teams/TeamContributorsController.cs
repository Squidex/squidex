// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities.Invitation;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Teams;

/// <summary>
/// Update and query teams.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Teams))]
public sealed class TeamContributorsController : ApiController
{
    private readonly IUserResolver userResolver;

    public TeamContributorsController(ICommandBus commandBus, IUserResolver userResolver)
        : base(commandBus)
    {
        this.userResolver = userResolver;
    }

    /// <summary>
    /// Get team contributors.
    /// </summary>
    /// <param name="team">The ID of the team.</param>
    /// <response code="200">Contributors returned.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpGet]
    [Route("teams/{team}/contributors/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamContributorsRead)]
    [ApiCosts(0)]
    public IActionResult GetContributors(string team)
    {
        var response = Deferred.AsyncResponse(() =>
        {
            return GetResponseAsync(Team, false);
        });

        Response.Headers[HeaderNames.ETag] = Team.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Assign contributor to team.
    /// </summary>
    /// <param name="team">The ID of the team.</param>
    /// <param name="request">Contributor object that needs to be added to the team.</param>
    /// <response code="201">Contributor assigned to team.</response>.
    /// <response code="400">Contributor request not valid.</response>.
    /// <response code="404">Team not found.</response>.
    [HttpPost]
    [Route("teams/{team}/contributors/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamContributorsAssign)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostContributor(string team, [FromBody] AssignContributorDto request)
    {
        var command = SimpleMapper.Map(request, new AssignContributor());

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetContributors), new { team }, response);
    }

    /// <summary>
    /// Remove yourself.
    /// </summary>
    /// <param name="team">The ID of the team.</param>
    /// <response code="200">Contributor removed.</response>.
    /// <response code="404">Contributor or team not found.</response>.
    [HttpDelete]
    [Route("teams/{team}/contributors/me/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteMyself(string team)
    {
        var command = new RemoveContributor { ContributorId = UserId };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Remove contributor.
    /// </summary>
    /// <param name="team">The ID of the team.</param>
    /// <param name="id">The ID of the contributor.</param>
    /// <response code="200">Contributor removed.</response>.
    /// <response code="404">Contributor or team not found.</response>.
    [HttpDelete]
    [Route("teams/{team}/contributors/{id}/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.TeamContributorsRevoke)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteContributor(string team, string id)
    {
        var command = new RemoveContributor { ContributorId = id };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<ContributorsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        if (context.PlainResult is InvitedResult<ITeamEntity> invited)
        {
            return await GetResponseAsync(invited.Entity, true);
        }
        else
        {
            return await GetResponseAsync(context.Result<ITeamEntity>(), false);
        }
    }

    private async Task<ContributorsDto> GetResponseAsync(ITeamEntity team, bool invited)
    {
        return await ContributorsDto.FromDomainAsync(team, Resources, userResolver, invited);
    }
}
