// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.AI;
using Squidex.Areas.Api.Controllers.Translations.Models;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities;
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
    private static readonly byte[] LineStart = Encoding.UTF8.GetBytes("data: ");
    private static readonly byte[] LineEnd = Encoding.UTF8.GetBytes("\r\r");
    private readonly IAssetStore assetStore;
    private readonly ITranslator translator;
    private readonly IChatAgent chatAgent;

    public TranslationsController(ICommandBus commandBus, IAssetStore assetStore, ITranslator translator, IChatAgent chatAgent)
        : base(commandBus)
    {
        this.assetStore = assetStore;
        this.translator = translator;
        this.chatAgent = chatAgent;
    }

    [OpenApiIgnore]
    [HttpGet("/ai-images/{*path}")]
    public IActionResult GetImage(string path)
    {
        return new FileCallbackResult("image/webp", async (body, range, ct) =>
        {
            await assetStore.DownloadAsync(path, body, range, ct);
        })
        {
            ErrorAs404 = true
        };
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
    [HttpGet]
    [Route("apps/{app}/ask/")]
    [ApiPermissionOrAnonymous(PermissionIds.AppTranslate)]
    [ApiCosts(10)]
    [OpenApiIgnore]
    public IActionResult GetQuestion(string app, AskDto request)
    {
        var chatRequest = new ChatRequest
        {
            Configuration = request.Configuration,
            ConversationId = request.ConversationId,
            Prompt = request.Prompt
        };

        var context = new AppChatContext
        {
            User = User,
            // Use a special context to provide access to the app.
            BaseContext = Context,
        };

        return new FileCallbackResult("text/event-stream", async (body, range, ct) =>
        {
            await foreach (var @event in chatAgent.StreamAsync(chatRequest, context, HttpContext.RequestAborted))
            {
                object? json = null;
                switch (@event)
                {
                    case ChunkEvent chunk:
                        json = new { type = "Chunk", content = chunk.Content };
                        break;
                    case ToolStartEvent toolStart:
                        json = new { type = "ToolStart", tool = toolStart.Tool.Spec.DisplayName };
                        break;
                    case ToolEndEvent toolEnd:
                        json = new { type = "ToolEnd", tool = toolEnd.Tool.Spec.DisplayName };
                        break;
                }

                if (json != null)
                {
                    await body.WriteAsync(LineStart, ct);
                    await JsonSerializer.SerializeAsync(body, json, cancellationToken: ct);
                    await body.WriteAsync(LineEnd, ct);

                    await body.FlushAsync(ct);
                }
            }
        });
    }
}
