// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Translations.Models
{
    public sealed class TranslateDto
    {
        /// <summary>
        /// The text to translate.
        /// </summary>
        [LocalizedRequired]
        public string Text { get; set; }

        /// <summary>
        /// The target language.
        /// </summary>
        [LocalizedRequired]
        public Language TargetLanguage { get; set; }

        /// <summary>
        /// The optional source language.
        /// </summary>
        public Language SourceLanguage { get; set; }
    }
}
