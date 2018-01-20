// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Languages
{
    /// <summary>
    /// Readonly API to the supported langauges.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [SwaggerTag(nameof(Languages))]
    public sealed class LanguagesController : ApiController
    {
        public LanguagesController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

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
