// ==========================================================================
//  AppLanguagesController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Controllers.Api.Apps.Models;
using Squidex.Infrastructure;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [Authorize(Roles = "app-owner")]
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

            var model = entity.Languages.Select(x => SimpleMapper.Map(x, new LanguageDto())).ToList();

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
        [HttpPost]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(LanguageDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostLanguage(string app, [FromBody] AddLanguageDto request)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(request, new AddLanguage()));

            var response = SimpleMapper.Map(request.Language, new LanguageDto());

            return StatusCode(201, response);
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
        [HttpPut]
        [Route("apps/{app}/languages/{language}")]
        public async Task<IActionResult> Update(string app, string language, [FromBody] SetMasterLanguageDto model)
        {
            if (model.IsMasterLanguage)
            {
                await CommandBus.PublishAsync(new SetMasterLanguage { Language = Language.GetLanguage(language) });
            }

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
        [HttpDelete]
        [Route("apps/{app}/languages/{language}")]
        public async Task<IActionResult> DeleteLanguage(string app, string language)
        {
            await CommandBus.PublishAsync(new RemoveLanguage { Language = Language.GetLanguage(language) });

            return NoContent();
        }
    }
}
