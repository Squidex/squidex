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
using Squidex.Infrastructure.Security;
using Squidex.Pipeline;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Backups
{
    public class RestoreController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public RestoreController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
        }

        /// <summary>
        /// Get current status.
        /// </summary>
        /// <returns>
        /// 200 => Status returned.
        /// </returns>
        [HttpGet]
        [Route("apps/restore/")]
        [ApiPermission(Permissions.AdminRestoreRead)]
        public async Task<IActionResult> GetJob()
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(User.OpenIdSubject());

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
        [ApiPermission(Permissions.AdminRestoreCreate)]
        public async Task<IActionResult> PostRestore([FromBody] RestoreRequest request)
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(User.OpenIdSubject());

            await restoreGrain.RestoreAsync(request.Url, request.Name);

            return NoContent();
        }
    }
}
