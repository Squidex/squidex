// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Apps
{
    /// <summary>
    /// Manages and configures apps.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Apps))]
    public sealed class AppLanguagesController : ApiController
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
        [HttpGet]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(AppLanguageDto[]), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public IActionResult GetLanguages(string app)
        {
            var response = AppLanguageDto.FromApp(App);

            Response.Headers[HeaderNames.ETag] = App.Version.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Attaches an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The language to add to the app.</param>
        /// <returns>
        /// 201 => Language created.
        /// 400 => Language request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/languages/")]
        [ProducesResponseType(typeof(AppLanguageDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiPermission(Permissions.AppLanguagesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostLanguage(string app, [FromBody] AddLanguageDto request)
        {
            var command = request.ToCommand();

            await CommandBus.PublishAsync(command);

            var response = AppLanguageDto.FromCommand(command);

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
        /// 400 => Language request not valid.
        /// 404 => Language or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/languages/{language}/")]
        [ApiPermission(Permissions.AppLanguagesUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> Update(string app, string language, [FromBody] UpdateLanguageDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand(ParseLanguage(language)));

            return NoContent();
        }

        /// <summary>
        /// Deletes an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to delete from the app.</param>
        /// <returns>
        /// 204 => Language deleted.
        /// 404 => Language or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/languages/{language}/")]
        [ApiPermission(Permissions.AppLanguagesDelete)]
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
