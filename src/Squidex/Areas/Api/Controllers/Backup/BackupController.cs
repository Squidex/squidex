// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Orleans;
using Squidex.Areas.Api.Controllers.Backup.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Tasks;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Backup
{
    /// <summary>
    /// Manages backups for app.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [MustBeAppOwner]
    [SwaggerTag(nameof(Backup))]
    public class BackupController : ApiController
    {
        private readonly IGrainFactory grainFactory;
        private readonly IAssetStore assetStore;

        public BackupController(ICommandBus commandBus, IGrainFactory grainFactory, IAssetStore assetStore)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
            this.assetStore = assetStore;
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
        [ProducesResponseType(typeof(List<BackupJobDto>), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetJobs(string app)
        {
            var backupGrain = grainFactory.GetGrain<IBackupGrain>(App.Id);

            var jobs = await backupGrain.GetStateAsync();

            return Ok(jobs.Value.Select(x => SimpleMapper.Map(x, new BackupJobDto())).ToList());
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
        [ApiCosts(0)]
        public IActionResult PostBackup(string app)
        {
            var backupGrain = grainFactory.GetGrain<IBackupGrain>(App.Id);

            backupGrain.RunAsync().Forget();

            return NoContent();
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
        [ProducesResponseType(200)]
        [ApiCosts(0.5)]
        public IActionResult GetBackupContent(string app, Guid id)
        {
            return new FileCallbackResult("application/zip", "Backup.zip", bodyStream => assetStore.DownloadAsync(id.ToString(), 0, null, bodyStream));
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
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteBackup(string app, Guid id)
        {
            var backupGrain = grainFactory.GetGrain<IBackupGrain>(App.Id);

            await backupGrain.DeleteAsync(id);

            return NoContent();
        }
    }
}
