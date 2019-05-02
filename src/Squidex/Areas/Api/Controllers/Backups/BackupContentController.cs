// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Assets;
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
        private readonly IAssetStore assetStore;
        private readonly IGrainFactory grainFactory;

        public BackupContentController(ICommandBus commandBus, IAssetStore assetStore, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.assetStore = assetStore;
            this.grainFactory = grainFactory;
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
        [ProducesResponseType(typeof(FileResult), 200)]
        [ApiCosts(0)]
        [AllowAnonymous]
        public async Task<IActionResult> GetBackupContent(string app, Guid id)
        {
            var backupGrain = grainFactory.GetGrain<IBackupGrain>(AppId);

            var backups = await backupGrain.GetStateAsync();
            var backup = backups.Value.Find(x => x.Id == id);

            if (backup == null || backup.Status != JobStatus.Completed)
            {
                return NotFound();
            }

            var fileName = $"backup-{app}-{backup.Started:yyyy-MM-dd_HH-mm-ss}";

            return new FileCallbackResult("application/zip", fileName, false, bodyStream =>
            {
                return assetStore.DownloadAsync(id.ToString(), 0, null, bodyStream);
            });
        }
    }
}
