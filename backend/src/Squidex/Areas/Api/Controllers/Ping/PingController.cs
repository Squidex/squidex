// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Ping
{
    /// <summary>
    /// Makes a ping request.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Ping))]
    public sealed class PingController : ApiController
    {
        private readonly ExposedValues exposedValues;

        public PingController(ICommandBus commandBus, ExposedValues exposedValues)
            : base(commandBus)
        {
            this.exposedValues = exposedValues;
        }

        /// <summary>
        /// Get API information.
        /// </summary>
        /// <returns>
        /// 200 => Infos returned.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(ExposedValues), StatusCodes.Status200OK)]
        [Route("info/")]
        public IActionResult GetInfo()
        {
            return Ok(exposedValues);
        }

        /// <summary>
        /// Get ping status of the API.
        /// </summary>
        /// <returns>
        /// 204 => Service ping successful.
        /// </returns>
        /// <remarks>
        /// Can be used to test, if the Squidex API is alive and responding.
        /// </remarks>
        [HttpGet]
        [Route("ping/")]
        public IActionResult GetPing()
        {
            return NoContent();
        }

        /// <summary>
        /// Get ping status.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 204 => Service ping successful.
        /// </returns>
        /// <remarks>
        /// Can be used to test, if the Squidex API is alive and responding.
        /// </remarks>
        [HttpGet]
        [Route("ping/{app}/")]
        [ApiPermissionOrAnonymous(Permissions.AppPing)]
        [ApiCosts(0)]
        public IActionResult GetAppPing(string app)
        {
            return NoContent();
        }
    }
}
