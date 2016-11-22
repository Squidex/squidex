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
using NSwag.Annotations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Modules.Api.Languages
{
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Languages", Description = "Readonly API to the supported langauges.")]
    public class LanguagesController : Controller
    {
        /// <summary>
        /// Get supported languages.
        /// </summary>
        /// <remarks>
        /// Provide a list of supported langauges code, following the ISO2Code standard.
        /// </remarks>
        /// <response code="200">Language codes returned.</response>
        [HttpGet]
        [Route("languages/")]
        [SwaggerTags("Languages")]
        [DescribedResponseType(200, typeof(string[]), "Supported languages returned.")]
        public IActionResult GetLanguages()
        {
            var model = Language.AllLanguages.Select(x => SimpleMapper.Map(x, new LanguageDto())).ToList();

            return Ok(model);
        }
    }
}
