// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dump;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Diagnostics
{
    /// <summary>
    /// Makes a diagnostics request.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Diagnostics))]
    public sealed class DiagnosticsController : ApiController
    {
        private readonly Dumper dumper;

        public DiagnosticsController(ICommandBus commandBus, Dumper dumper)
            : base(commandBus)
        {
            this.dumper = dumper;
        }

        /// <summary>
        /// Creates a dump and writes it into storage..
        /// </summary>
        /// <returns>
        /// 204 => Dump created successful.
        /// 501 => Not configured.
        /// </returns>
        [HttpGet]
        [Route("diagnostics/dump")]
        [ApiPermissionOrAnonymous(Permissions.Admin)]
        public async Task<IActionResult> GetDump()
        {
            var success = await dumper.CreateDumpAsync(HttpContext.RequestAborted);

            if (!success)
            {
                return StatusCode(501);
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a gc dump and writes it into storage.
        /// </summary>
        /// <returns>
        /// 204 => Dump created successful.
        /// 501 => Not configured.
        /// </returns>
        [HttpGet]
        [Route("diagnostics/gc-dump")]
        [ApiPermissionOrAnonymous(Permissions.Admin)]
        public async Task<IActionResult> GetGCDump()
        {
            var success = await dumper.CreateGCDumpAsync(HttpContext.RequestAborted);

            if (!success)
            {
                return StatusCode(501);
            }

            return NoContent();
        }
    }
}
