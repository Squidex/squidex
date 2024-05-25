// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;

namespace Squidex.Areas.Api.Controllers.Translations.Models;

public sealed class AskDto
{
    /// <summary>
    /// Optional conversation ID.
    /// </summary>
    [FromQuery(Name = "conversationId")]
    public string? ConversationId { get; set; }

    /// <summary>
    /// Optional configuration.
    /// </summary>
    [FromQuery(Name = "configuration")]
    public string? Configuration { get; set; }

    /// <summary>
    /// The text to ask.
    /// </summary>
    [FromQuery(Name = "prompt")]
    public string? Prompt { get; set; }
}
