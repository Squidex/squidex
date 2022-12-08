// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Invitation;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppContributorsController : ApiController
{
    private readonly IUsageGate usageGate;
    private readonly IUserResolver userResolver;

    public AppContributorsController(ICommandBus commandBus, IUsageGate usageGate, IUserResolver userResolver)
        : base(commandBus)
    {
        this.usageGate = usageGate;
        this.userResolver = userResolver;
    }

    /// <summary>
    /// Get app contributors.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Contributors returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/contributors/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppContributorsRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetContributors(string app)
    {
        var response = await GetResponseAsync(App, false);

        return Ok(response);
    }

    /// <summary>
    /// Assign contributor to app.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">Contributor object that needs to be added to the app.</param>
    /// <response code="201">Contributor assigned to app.</response>.
    /// <response code="400">Contributor request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/contributors/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.AppContributorsAssign)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostContributor(string app, [FromBody] AssignContributorDto request)
    {
        var command = SimpleMapper.Map(request, new AssignContributor());

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetContributors), new { app }, response);
    }

    /// <summary>
    /// Remove yourself.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Contributor removed.</response>.
    /// <response code="404">Contributor or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/contributors/me/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteMyself(string app)
    {
        var command = new RemoveContributor { ContributorId = UserId };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Remove contributor.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the contributor.</param>
    /// <response code="200">Contributor removed.</response>.
    /// <response code="404">Contributor or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/contributors/{id}/")]
    [ProducesResponseType(typeof(ContributorsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppContributorsRevoke)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteContributor(string app, string id)
    {
        var command = new RemoveContributor { ContributorId = id };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<ContributorsDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        if (context.PlainResult is InvitedResult<IAppEntity> invited)
        {
            return await GetResponseAsync(invited.Entity, true);
        }
        else
        {
            return await GetResponseAsync(context.Result<IAppEntity>(), false);
        }
    }

    private async Task<ContributorsDto> GetResponseAsync(IAppEntity app, bool invited)
    {
        var (plan, _, _) = await usageGate.GetPlanForAppAsync(app, false, HttpContext.RequestAborted);

        return await ContributorsDto.FromDomainAsync(app, Resources, userResolver, plan, invited);
    }
}
