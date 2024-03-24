// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Translations.Models;

[OpenApiRequest]
public sealed class AskDto
{
    /// <summary>
    /// Optional conversation ID.
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// The text to ask.
    /// </summary>
    [LocalizedRequired]
    public string Prompt { get; set; }
}
