// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Squidex.Areas.IdentityServer.Controllers.Error
{
    [SwaggerIgnore]
    public sealed class ErrorController : IdentityServerController
    {
        [Route("error/")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
