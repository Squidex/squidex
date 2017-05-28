// ==========================================================================
//  ErrorController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;

namespace Squidex.Controllers.UI.Error
{
    public class ErrorController : Controller
    {
        [Route("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
