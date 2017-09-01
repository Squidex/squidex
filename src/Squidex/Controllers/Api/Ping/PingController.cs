// ==========================================================================
//  PingController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Ping
{
    /// <summary>
    /// Makes a ping request.
    /// </summary>
    [MustBeAppReader]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("Ping")]
    public class PingController : Controller
    {
        /// <summary>
        /// Get ping status.
        /// </summary>
        /// <returns>
        /// 204 => Service ping successful.
        /// </returns>
        /// <remarks>
        /// Can be used to test, if the Squidex API is alive and responding.
        /// </remarks>
        [HttpGet]
        [Route("ping/{app}/")]
        [ApiCosts(0)]
        public IActionResult GetPing()
        {
            return NoContent();
        }
    }
}
