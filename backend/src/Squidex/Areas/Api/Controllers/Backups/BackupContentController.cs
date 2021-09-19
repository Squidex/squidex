// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Manages backups for app.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Backups))]
    public class BackupContentController : ApiController
    {
        private readonly IBackupArchiveStore backupArchiveStore;
        private readonly IBackupService backupservice;

        public BackupContentController(ICommandBus commandBus,
            IBackupArchiveStore backupArchiveStore,
            IBackupService backupservice)
            : base(commandBus)
        {
            this.backupArchiveStore = backupArchiveStore;
            this.backupservice = backupservice;
        }

        /// <summary>
        /// Get the backup content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <returns>
        /// 200 => Backup found and content returned.
        /// 404 => Backup or app not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/backups/{id}")]
        [ResponseCache(Duration = 3600 * 24 * 30)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ApiCosts(0)]
        [AllowAnonymous]
        public async Task<IActionResult> GetBackupContent(string app, DomainId id)
        {
            var backup = await backupservice.GetBackupAsync(AppId, id, HttpContext.RequestAborted);

            if (backup == null || backup.Status != JobStatus.Completed)
            {
                return NotFound();
            }

            var fileName = $"backup-{app}-{backup.Started:yyyy-MM-dd_HH-mm-ss}.zip";

            var callback = new FileCallback((body, range, ct) =>
            {
                return backupArchiveStore.DownloadAsync(id, body, ct);
            });

            return new FileCallbackResult("application/zip", callback)
            {
                FileDownloadName = fileName
            };
        }
    }
}
