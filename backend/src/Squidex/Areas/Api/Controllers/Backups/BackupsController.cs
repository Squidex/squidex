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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex.Areas.Api.Controllers.Backups;

/// <summary>
/// Update and query backups for apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Backups))]
public class BackupsController : ApiController
{
    private readonly IJobService jobService;

    public BackupsController(ICommandBus commandBus, IJobService jobService)
        : base(commandBus)
    {
        this.jobService = jobService;
    }

    /// <summary>
    /// Get all backup jobs.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Backups returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/backups/")]
    [ProducesResponseType(typeof(BackupJobsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppJobsRead)]
    [ApiCosts(0)]
    [Obsolete("Use Jobs endpoint.")]
    public async Task<IActionResult> GetBackups(string app)
    {
        var jobs = await jobService.GetJobsAsync(App.Id, HttpContext.RequestAborted);

        var result = BackupJobsDto.FromDomain(jobs.Where(x => x.TaskName == BackupJob.TaskName), Resources);

        return Ok(result);
    }

    /// <summary>
    /// Start a new backup.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="204">Backup started.</response>
    /// <response code="400">Backup contingent reached.</response>
    /// <response code="404">App not found.</response>
    [HttpPost]
    [Route("apps/{app}/backups/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppJobsCreate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PostBackup(string app)
    {
        var job = BackupJob.BuildRequest(User.Token()!, App);

        await jobService.StartAsync(App.Id, job, default);

        return NoContent();
    }

    /// <summary>
    /// Delete a backup.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the backup to delete.</param>
    /// <response code="204">Backup deleted.</response>
    /// <response code="404">Backup or app not found.</response>
    [HttpDelete]
    [Route("apps/{app}/backups/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppJobsDelete)]
    [ApiCosts(0)]
    [Obsolete("Use Jobs endpoint.")]
    public async Task<IActionResult> DeleteBackup(string app, DomainId id)
    {
        await jobService.DeleteJobAsync(App.Id, id, default);

        return NoContent();
    }
}
