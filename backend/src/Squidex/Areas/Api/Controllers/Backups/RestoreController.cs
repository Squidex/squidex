// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups;

/// <summary>
/// Update and query backups for apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Backups))]
[ApiModelValidation(true)]
public class RestoreController : ApiController
{
    private readonly IJobService jobService;

    public RestoreController(ICommandBus commandBus, IJobService jobService)
        : base(commandBus)
    {
        this.jobService = jobService;
    }

    /// <summary>
    /// Get current restore status.
    /// </summary>
    /// <response code="200">Status returned.</response>
    [HttpGet]
    [Route("apps/restore/")]
    [ProducesResponseType(typeof(RestoreJobDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminRestore)]
    public async Task<IActionResult> GetRestoreJob()
    {
        var jobs = await jobService.GetJobsAsync(default, HttpContext.RequestAborted);
        var job = jobs.Find(x => x.TaskName == RestoreJob.TaskName);

        if (job == null)
        {
            return Ok(new RestoreJobDto());
        }

        var response = RestoreJobDto.FromDomain(job);

        return Ok(response);
    }

    /// <summary>
    /// Restore a backup.
    /// </summary>
    /// <param name="request">The backup to restore.</param>
    /// <response code="204">Restore operation started.</response>
    [HttpPost]
    [Route("apps/restore/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission(PermissionIds.AdminRestore)]
    public async Task<IActionResult> PostRestoreJob([FromBody] RestoreRequestDto request)
    {
        var job = RestoreJob.BuildRequest(User.Token()!, request.Url, request.Name);

        await jobService.StartAsync(default, job, HttpContext.RequestAborted);

        return NoContent();
    }
}
