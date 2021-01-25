// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Translations.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Text.Translations;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Translations
{
    /// <summary>
    /// Manage translations.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Translations))]
    public sealed class TranslationsController : ApiController
    {
        private readonly ITranslator translator;

        public TranslationsController(ICommandBus commandBus, ITranslator translator)
            : base(commandBus)
        {
            this.translator = translator;
        }

        /// <summary>
        /// Translate a text.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The translation request.</param>
        /// <returns>
        /// 200 => Text translated.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/translations/")]
        [ProducesResponseType(typeof(TranslationDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppTranslate)]
        [ApiCosts(0)]
        public async Task<IActionResult> PostTranslation(string app, [FromBody] TranslateDto request)
        {
            var result = await translator.TranslateAsync(request.Text, request.TargetLanguage, request.SourceLanguage, HttpContext.RequestAborted);
            var response = TranslationDto.FromTranslation(result);

            return Ok(response);
        }
    }
}
