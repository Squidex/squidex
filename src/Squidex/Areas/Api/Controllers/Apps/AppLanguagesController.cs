﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

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
        [ProducesResponseType(typeof(AppLanguagesDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public IActionResult GetLanguages(string app)
        {
            var response = Deferred.Response(() =>
            {
                return AppLanguagesDto.FromApp(App, this);
            });

            Response.Headers[HeaderNames.ETag] = App.ToEtag();

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
        [ProducesResponseType(typeof(AppLanguagesDto), 201)]
        [ApiPermission(Permissions.AppLanguagesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostLanguage(string app, [FromBody] AddLanguageDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetLanguages), new { app }, response);
        }

        /// <summary>
        /// Updates an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to update.</param>
        /// <param name="request">The language object.</param>
        /// <returns>
        /// 200 => Language updated.
        /// 400 => Language request not valid.
        /// 404 => Language or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/languages/{language}/")]
        [ProducesResponseType(typeof(AppLanguagesDto), 200)]
        [ApiPermission(Permissions.AppLanguagesUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutLanguage(string app, string language, [FromBody] UpdateLanguageDto request)
        {
            var command = request.ToCommand(ParseLanguage(language));

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Deletes an app language.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="language">The language to delete from the app.</param>
        /// <returns>
        /// 200 => Language deleted.
        /// 404 => Language or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/languages/{language}/")]
        [ProducesResponseType(typeof(AppLanguagesDto), 200)]
        [ApiPermission(Permissions.AppLanguagesDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteLanguage(string app, string language)
        {
            var command = new RemoveLanguage { Language = ParseLanguage(language) };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        private async Task<AppLanguagesDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IAppEntity>();
            var response = AppLanguagesDto.FromApp(result, this);

            return response;
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
