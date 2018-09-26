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
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Backups
{
    /// <summary>
    /// Restores backups.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [ApiModelValidation(true)]
    [MustBeAdministrator]
    [SwaggerIgnore]
    public class RestoreController : ApiController
    {
        private readonly IGrainFactory grainFactory;

        public RestoreController(ICommandBus commandBus, IGrainFactory grainFactory)
            : base(commandBus)
        {
            this.grainFactory = grainFactory;
        }

        [HttpGet]
        [Route("apps/restore/")]
        [ApiCosts(0)]
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

        [HttpPost]
        [Route("apps/restore/")]
        [ApiCosts(0)]
        public async Task<IActionResult> PostRestore([FromBody] RestoreRequest request)
        {
            var restoreGrain = grainFactory.GetGrain<IRestoreGrain>(User.OpenIdSubject());

            await restoreGrain.RestoreAsync(request.Url, request.Name);

            return NoContent();
        }
    }
}
