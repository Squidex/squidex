// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Translations.Models
{
    public sealed class TranslateDto
    {
        /// <summary>
        /// The text to translate.
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// The target language.
        /// </summary>
        [Required]
        public Language TargetLanguage { get; set; }

        /// <summary>
        /// The optional source language.
        /// </summary>
        public Language SourceLanguage { get; set; }
    }
}
