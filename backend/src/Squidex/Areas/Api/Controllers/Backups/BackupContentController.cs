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

namespace Squidex.Areas.Api.Controllers.Backups;

/// <summary>
/// Update and query backups for app.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Backups))]
[Obsolete("Use Jobs endpoint.")]
public class BackupContentController : ApiController
{
    private readonly IJobService jobService;

    public BackupContentController(ICommandBus commandBus,
        IJobService jobService)
        : base(commandBus)
    {
        this.jobService = jobService;
    }

    /// <summary>
    /// Get the backup content.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the backup.</param>
    /// <response code="200">Backup found and content returned.</response>
    /// <response code="404">Backup or app not found.</response>
    [HttpGet]
    [Route("apps/{app}/backups/{id}")]
    [ResponseCache(Duration = 3600 * 24 * 30)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ApiCosts(0)]
    [AllowAnonymous]
    [Obsolete("Use Jobs endpoint.")]
    public Task<IActionResult> GetBackupContent(string app, DomainId id)
    {
        return GetBackupAsync(AppId, app, id);
    }

    /// <summary>
    /// Get the backup content.
    /// </summary>
    /// <param name="id">The ID of the backup.</param>
    /// <param name="appId">The ID of the app.</param>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Backup found and content returned.</response>
    /// <response code="404">Backup or app not found.</response>
    [HttpGet]
    [Route("apps/backups/{id}")]
    [ResponseCache(Duration = 3600 * 24 * 30)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ApiCosts(0)]
    [AllowAnonymous]
    [Obsolete("Use Jobs endpoint.")]
    public Task<IActionResult> GetBackupContentV2(DomainId id, [FromQuery] DomainId appId = default, [FromQuery] string app = "")
    {
        return GetBackupAsync(appId, app, id);
    }

    private async Task<IActionResult> GetBackupAsync(DomainId appId, string app, DomainId id)
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
