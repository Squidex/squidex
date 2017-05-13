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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Core;
using Squidex.Core.Identity;
using Squidex.Infrastructure;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Apps")]
    public class AppLanguagesController : ControllerBase
    {
        private readonly IAppProvider appProvider;

        public AppLanguagesController(ICommandBus commandBus, IAppProvider appProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
        }

        /// <summary>
        /// Get app languages.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Language configuration returned.
        /// 404 => App not found.
        /// </returns>
        [Authorize(Roles = SquidexRoles.AppEditor)]
        [HttpGet]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(LanguageDto[]), 200)]
        public async Task<IActionResult> GetLanguages(string app)
        {
            var entity = await appProvider.FindAppByNameAsync(app);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.LanguagesConfig.OfType<LanguageConfig>().Select(x => 
                SimpleMapper.Map(x.Language, 
                    new AppLanguageDto
                    {
                        IsMaster = x == entity.LanguagesConfig.Master,
                        IsOptional = x.IsOptional,
                        Fallback = x.Fallback.ToList()
                    })).OrderByDescending(x => x.IsMaster).ThenBy(x => x.Iso2Code).ToList();

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(model);
        }

        /// <summary>
        /// Attaches an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The language to add to the app.</param>
        /// <returns>
        /// 201 => App language created.
        /// 400 => Language is an invalid language.
        /// 404 => App not found.
        /// </returns>
        [Authorize(Roles = SquidexRoles.AppOwner)]
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
        /// <param name="language">The language to delete from the app.</param>
        /// <param name="model">The language properties.</param>
        /// <returns>
        /// 204 => App language updated.
        /// 400 => Language is an invalid language.
        /// 404 => App not found.
        /// </returns>
        [Authorize(Roles = SquidexRoles.AppOwner)]
        [HttpPut]
        [Route("apps/{app}/languages/{language}")]
        [ApiCosts(1)]
        public async Task<IActionResult> Update(string app, string language, [FromBody] UpdateAppLanguageDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new UpdateLanguage { Language = language }));
          
            return NoContent();
        }

        /// <summary>
        /// Deletes an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to delete from the app.</param>
        /// <returns>
        /// 204 => App language deleted.
        /// 400 => Language is an invalid language.
        /// 404 => App not found.
        /// </returns>
        [Authorize(Roles = SquidexRoles.AppOwner)]
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
