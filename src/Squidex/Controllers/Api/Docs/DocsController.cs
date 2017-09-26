// ==========================================================================
//  DocsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Docs
{
    [SwaggerIgnore]
    public sealed class DocsController : Controller
    {
        [HttpGet]
        [Route("docs/")]
        [ApiCosts(0)]
        public IActionResult Docs()
        {
            var vm = new DocsVM { Specification = "~/swagger/v1/swagger.json" };

            return View(nameof(Docs), vm);
        }
    }
}
