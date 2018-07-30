// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Orleans;
using Squidex.Areas.Api.Controllers.Backups.Models;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Restores backups.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [ApiModelValidation(true)]
    [SwaggerTag(nameof(Backups))]
    public class RestoreController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public RestoreController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
        }

        /// <summary>
        /// Get the restore jobs.
        /// </summary>
        /// <returns>
        /// 200 => Restore job returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/restore/")]
        [ProducesResponseType(typeof(RestoreJobDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetJob()
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(User.OpenIdSubject());

            var job = await restoreGrain.GetJobAsync();

            if (job.Value == null)
            {
                return NotFound();
            }

            var jobs = await restoreGrain.GetJobAsync();

            var response = RestoreJobDto.FromJob(job.Value);

            return Ok(response);
        }

        /// <summary>
        /// Start a new restore job.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>
        /// 204 => Backup started.
        /// </returns>
        [HttpPost]
        [Route("apps/restore/")]
        [ApiCosts(0)]
        public async Task<IActionResult> PostRestore([FromBody] RestoreRequest request)
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(User.OpenIdSubject());

            await restoreGrain.RestoreAsync(request.Url);

            return NoContent();
        }
    }
}
