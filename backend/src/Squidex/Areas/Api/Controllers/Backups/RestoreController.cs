// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
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
    private readonly IBackupService backupService;

    public RestoreController(ICommandBus commandBus, IBackupService backupService)
        : base(commandBus)
    {
        this.backupService = backupService;
    }

    /// <summary>
    /// Get current restore status.
    /// </summary>
    /// <response code="200">Status returned.</response>.
    [HttpGet]
    [Route("apps/restore/")]
    [ProducesResponseType(typeof(RestoreJobDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminRestore)]
    public async Task<IActionResult> GetRestoreJob()
    {
        var job = await backupService.GetRestoreAsync(HttpContext.RequestAborted);

        if (job == null)
        {
            return NotFound();
        }

        var response = RestoreJobDto.FromDomain(job);

        return Ok(response);
    }

    /// <summary>
    /// Restore a backup.
    /// </summary>
    /// <param name="request">The backup to restore.</param>
    /// <response code="204">Restore operation started.</response>.
    [HttpPost]
    [Route("apps/restore/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission(PermissionIds.AdminRestore)]
    public async Task<IActionResult> PostRestoreJob([FromBody] RestoreRequestDto request)
    {
        await backupService.StartRestoreAsync(User.Token()!, request.Url, request.Name, HttpContext.RequestAborted);

        return NoContent();
    }
}
