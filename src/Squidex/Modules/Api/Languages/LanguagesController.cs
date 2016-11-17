// =========================================================================
//  LanguagesController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure;
using Squidex.Pipeline;

namespace Squidex.Modules.Api.Languages
{
    [Authorize]
    [ApiExceptionFilter]
    public class LanguagesController : Controller
    {
        [HttpGet]
        [Route("languages/")]
        public IActionResult GetLanguages()
        {
            var model = Language.AllLanguages.ToList();

            return Ok(model);
        }
    }
}
