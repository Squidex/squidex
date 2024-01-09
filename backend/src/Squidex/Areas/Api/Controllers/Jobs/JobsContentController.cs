// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Jobs;

/// <summary>
/// Update and query jobs for app.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Jobs))]
public class JobsContentController : ApiController
{
    private readonly IJobService jobService;

    public JobsContentController(ICommandBus commandBus,
        IJobService jobService)
        : base(commandBus)
    {
        this.jobService = jobService;
    }

    /// <summary>
    /// Get the job content.
    /// </summary>
    /// <param name="id">The ID of the job.</param>
    /// <param name="appId">The ID of the app.</param>
    /// <response code="200">Job found and content returned.</response>
    /// <response code="404">Job or app not found.</response>
    [HttpGet]
    [Route("apps/jobs/{id}")]
    [ResponseCache(Duration = 3600 * 24 * 30)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ApiCosts(0)]
    [AllowAnonymous]
    public async Task<IActionResult> GetJobContent(DomainId id, [FromQuery] DomainId appId = default)
    {
        var jobs = await jobService.GetJobsAsync(appId, HttpContext.RequestAborted);
        var job = jobs.Find(x => x.Id == id);

        if (job is not { Status: JobStatus.Completed } || job.File == null)
        {
            return NotFound();
        }

        var callback = new FileCallback((body, range, ct) =>
        {
            return jobService.DownloadAsync(job, body, ct);
        });

        return new FileCallbackResult(job.File.MimeType, callback)
        {
            FileDownloadName = job.File.Name,
            FileSize = null,
            ErrorAs404 = true
        };
    }
}
