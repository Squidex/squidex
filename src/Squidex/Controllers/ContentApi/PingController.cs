// ==========================================================================
//  PingController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Core.Identity;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    [Authorize(Roles = SquidexRoles.AppEditor)]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    public class PingController : Controller
    {
        [HttpGet]
        [Route("ping/{app}/")]
        public IActionResult GetPing()
        {
            return Ok();
        }
    }
}
