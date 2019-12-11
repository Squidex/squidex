﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Manages backups for apps.
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
        /// <returns>
        /// 200 => Backups returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/backups/")]
        [ProducesResponseType(typeof(BackupJobsDto), 200)]
        [ApiPermission(Permissions.AppBackupsRead)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetBackups(string app)
        {
            var jobs = await backupService.GetBackupsAsync(AppId);

            var response = BackupJobsDto.FromBackups(jobs, this, app);

            return Ok(response);
        }

        /// <summary>
        /// Start a new backup.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 204 => Backup started.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/backups/")]
        [ProducesResponseType(typeof(List<BackupJobDto>), 200)]
        [ApiPermission(Permissions.AppBackupsCreate)]
        [ApiCosts(0)]
        public IActionResult PostBackup(string app)
        {
            backupService.StartBackupAsync(App.Id, User.Token()!).Forget();

            return NoContent();
        }

        /// <summary>
        /// Delete a backup.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the backup to delete.</param>
        /// <returns>
        /// 204 => Backup started.
        /// 404 => Backup or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/backups/{id}")]
        [ProducesResponseType(typeof(List<BackupJobDto>), 200)]
        [ApiPermission(Permissions.AppBackupsDelete)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteBackup(string app, Guid id)
        {
            await backupService.DeleteBackupAsync(AppId, id);

            return NoContent();
        }
    }
}
