// ==========================================================================
//  AppLanguagesController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("Apps")]
    public class AppLanguagesController : ControllerBase
    {
        public AppLanguagesController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get app languages.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Language configuration returned.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppEditor]
        [HttpGet]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(LanguageDto[]), 200)]
        public IActionResult GetLanguages(string app)
        {
            var response = App.LanguagesConfig.OfType<LanguageConfig>().Select(x =>
                SimpleMapper.Map(x.Language,
                    new AppLanguageDto
                    {
                        IsMaster = x == App.LanguagesConfig.Master,
                        IsOptional = x.IsOptional,
                        Fallback = x.Fallback.ToList()
                    })).OrderByDescending(x => x.IsMaster).ThenBy(x => x.Iso2Code).ToList();

            Response.Headers["ETag"] = new StringValues(App.Version.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Attaches an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The language to add to the app.</param>
        /// <returns>
        /// 201 => Language created.
        /// 400 => Language is an invalid language.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppOwner]
        [HttpPost]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(AppLanguageDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostLanguage(string app, [FromBody] AddAppLanguageDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new AddLanguage()));

            var response = SimpleMapper.Map(request.Language, new AppLanguageDto { Fallback = new List<Language>() });

            return CreatedAtAction(nameof(GetLanguages), new { app }, response);
        }

        /// <summary>
        /// Updates an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to update.</param>
        /// <param name="request">The language object.</param>
        /// <returns>
        /// 204 => Language updated.
        /// 400 => Language object is invalid.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppOwner]
        [HttpPut]
        [Route("apps/{app}/languages/{language}")]
        [ApiCosts(1)]
        public async Task<IActionResult> Update(string app, string language, [FromBody] UpdateAppLanguageDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new UpdateLanguage { Language = language }));

            return NoContent();
        }

        /// <summary>
        /// Deletes an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to delete from the app.</param>
        /// <returns>
        /// 204 => Language deleted.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppOwner]
        [HttpDelete]
        [Route("apps/{app}/languages/{language}")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteLanguage(string app, string language)
        {
            await CommandBus.PublishAsync(new RemoveLanguage { Language = ParseLanguage(language) });

            return NoContent();
        }

        private static Language ParseLanguage(string language)
        {
            try
            {
                return Language.GetLanguage(language);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException($"Language '{language}' is not valid.");
            }
        }
    }
}
