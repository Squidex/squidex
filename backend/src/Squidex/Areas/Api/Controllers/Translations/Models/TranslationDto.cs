// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;
using Squidex.Text.Translations;

namespace Squidex.Areas.Api.Controllers.Translations.Models;

public sealed class TranslationDto
{
    /// <summary>
    /// The result of the translation.
    /// </summary>
    public TranslationResultCode Result { get; set; }

    /// <summary>
    /// The translated text.
    /// </summary>
    public string? Text { get; set; }

    public static TranslationDto FromDomain(TranslationResult translation)
    {
        return SimpleMapper.Map(translation, new TranslationDto { Result = translation.Code });
    }
}
