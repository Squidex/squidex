// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Jobs.Models;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Jobs;

/// <summary>
/// Update and query jobs for apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Jobs))]
public class JobsController : ApiController
{
    private readonly IJobService jobService;

    public JobsController(ICommandBus commandBus, IJobService jobService)
        : base(commandBus)
    {
        this.jobService = jobService;
    }

    /// <summary>
    /// Get all jobs.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Jobs returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/jobs/")]
    [ProducesResponseType(typeof(JobsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppJobsRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetJobs(string app)
    {
        var jobs = await jobService.GetJobsAsync(App.Id, HttpContext.RequestAborted);

        var result = JobsDto.FromDomain(jobs, Resources);

        return Ok(result);
    }

    /// <summary>
    /// Delete a job.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the jobs to delete.</param>
    /// <response code="204">Job deleted.</response>
    /// <response code="404">Job or app not found.</response>
    [HttpDelete]
    [Route("apps/{app}/jobs/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppJobsDelete)]
    [ApiCosts(0)]
    public async Task<IActionResult> DeleteJob(string app, DomainId id)
    {
        await jobService.DeleteJobAsync(App.Id, id, default);

        return NoContent();
    }
}
