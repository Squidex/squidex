// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.AI;
using Squidex.Areas.Api.Controllers.Translations.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Text.Translations;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Translations;

/// <summary>
/// Manage translations.
/// </summary>
[ApiModelValidation(true)]
[ApiExplorerSettings(GroupName = nameof(Translations))]
public sealed class TranslationsController : ApiController
{
    private readonly ITranslator translator;
    private readonly IChatAgent chatAgent;

    public TranslationsController(ICommandBus commandBus, ITranslator translator, IChatAgent chatAgent)
        : base(commandBus)
    {
        this.translator = translator;
        this.chatAgent = chatAgent;
    }

    /// <summary>
    /// Translate a text.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The translation request.</param>
    /// <response code="200">Text translated.</response>
    [HttpPost]
    [Route("apps/{app}/translations/")]
    [ProducesResponseType(typeof(TranslationDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppTranslate)]
    [ApiCosts(10)]
    public async Task<IActionResult> PostTranslation(string app, [FromBody] TranslateDto request)
    {
        var result = await translator.TranslateAsync(request.Text, request.TargetLanguage, request.SourceLanguage, HttpContext.RequestAborted);
        var response = TranslationDto.FromDomain(result);

        return Ok(response);
    }

    /// <summary>
    /// Asks the chatbot a question a text.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The question request.</param>
    /// <response code="200">Question asked.</response>
    [HttpPost]
    [Route("apps/{app}/ask/")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppTranslate)]
    [ApiCosts(10)]
    public async Task<IActionResult> PostQuestion(string app, [FromBody] AskDto request)
    {
        var result = await chatAgent.PromptAsync(request.Prompt, request.ConversationId, HttpContext.RequestAborted);
        var response = new string[] { result.Text };

        return Ok(response);
    }
}
