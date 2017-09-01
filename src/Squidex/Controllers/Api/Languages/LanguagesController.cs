// ==========================================================================
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

namespace Squidex.Controllers.Api.Languages
{
    /// <summary>
    /// Readonly API to the supported langauges.
    /// </summary>
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Languages")]
    public sealed class LanguagesController : Controller
    {
        /// <summary>
        /// Get supported languages.
        /// </summary>
        /// <remarks>
        /// Provide a list of supported langauges code, following the ISO2Code standard.
        /// </remarks>
        /// <returns>
        /// 200 => Supported language codes returned.
        /// </returns>
        [HttpGet]
        [Route("languages/")]
        [ProducesResponseType(typeof(string[]), 200)]
        [ApiCosts(0)]
        public IActionResult GetLanguages()
        {
            var response = Language.AllLanguages.Select(x => SimpleMapper.Map(x, new LanguageDto())).ToList();

            return Ok(response);
        }
    }
}
