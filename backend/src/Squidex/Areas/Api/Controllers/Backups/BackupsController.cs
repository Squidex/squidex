// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups;

/// <summary>
/// Update and query backups for apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Backups))]
public class BackupsController : ApiController
{
    private readonly IBackupService backupService;

    public BackupsController(ICommandBus commandBus, IBackupService backupService)
        : base(commandBus)
    {
        this.backupService = backupService;
    }

    /// <summary>
    /// Get all backup jobs.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Backups returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/backups/")]
    [ProducesResponseType(typeof(BackupJobsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppBackupsRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetBackups(string app)
    {
        var jobs = await backupService.GetBackupsAsync(AppId, HttpContext.RequestAborted);

        var response = BackupJobsDto.FromDomain(jobs, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Start a new backup.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="204">Backup started.</response>.
    /// <response code="400">Backup contingent reached.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/backups/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppBackupsCreate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PostBackup(string app)
    {
        await backupService.StartBackupAsync(App.Id, User.Token()!, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Delete a backup.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the backup to delete.</param>
    /// <response code="204">Backup deleted.</response>.
    /// <response code="404">Backup or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/backups/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppBackupsDelete)]
    [ApiCosts(0)]
    public async Task<IActionResult> DeleteBackup(string app, DomainId id)
    {
        await backupService.DeleteBackupAsync(AppId, id, HttpContext.RequestAborted);

        return NoContent();
    }
}
