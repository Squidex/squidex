// ==========================================================================
//  ErrorController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Squidex.Controllers.UI.Error
{
    [SwaggerIgnore]
    public sealed class ErrorController : Controller
    {
        [Route("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
