// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Manages backups for apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Backups))]
    public class RestoreController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public RestoreController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
        }

        /// <summary>
        /// Get current restore status.
        /// </summary>
        /// <returns>
        /// 200 => Status returned.
        /// </returns>
        [HttpGet]
        [Route("apps/restore/")]
        [ProducesResponseType(typeof(RestoreJobDto), 200)]
        [ApiPermission(Permissions.AdminRestore)]
        public async Task<IActionResult> GetJob()
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id);

            var job = await restoreGrain.GetJobAsync();

            if (job.Value == null)
            {
                return NotFound();
            }

            var response = RestoreJobDto.FromJob(job.Value);

            return Ok(response);
        }

        /// <summary>
        /// Restore a backup.
        /// </summary>
        /// <param name="request">The backup to restore.</param>
        /// <returns>
        /// 204 => Restore operation started.
        /// </returns>
        [HttpPost]
        [Route("apps/restore/")]
        [ApiPermission(Permissions.AdminRestore)]
        public async Task<IActionResult> PostRestore([FromBody] RestoreRequestDto request)
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id);

            await restoreGrain.RestoreAsync(request.Url, User.Token(), request.Name);

            return NoContent();
        }
    }
}
