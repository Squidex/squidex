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
using Squidex.Modules.Api.Apps.Models;
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Write.Apps.Commands;

namespace Squidex.Modules.Api.Apps
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
        /// Configures the app languages.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="model">The language configuration for the app.</param>
        /// <returns>
        /// 201 => App languages configured.
        /// 400 => Language configuration is empty or contains an invalid language.
        /// </returns>
        /// <remarks>
        /// The ordering of the languages matterns: When you retrieve a content with a localized content squidex tries
        /// to resolve the correct language for these properties. When there is no value for a property in the specified language,
        /// the previous languages from languages list is uses as a fallback. 
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(ErrorDto[]), 400)]
        public async Task<IActionResult> PostLanguages(string app, [FromBody] ConfigureLanguagesDto model)
        {
            await CommandBus.PublishAsync(SimpleMapper.Map(model, new ConfigureLanguages()));

            return Ok();
        }
    }
}
