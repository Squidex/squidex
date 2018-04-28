// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers
{
    public sealed class LanguageDto
    {
        /// <summary>
        /// The iso code of the language.
        /// </summary>
        [Required]
        public string Iso2Code { get; set; }

        /// <summary>
        /// The english name of the language.
        /// </summary>
        [Required]
        public string EnglishName { get; set; }

        public static LanguageDto FromLanguage(Language language)
        {
            return SimpleMapper.Map(language, new LanguageDto());
        }
    }
}
