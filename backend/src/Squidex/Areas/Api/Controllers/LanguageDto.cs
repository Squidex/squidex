// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers
{
    public sealed class LanguageDto
    {
        /// <summary>
        /// The iso code of the language.
        /// </summary>
        [LocalizedRequired]
        public string Iso2Code { get; set; }

        /// <summary>
        /// The english name of the language.
        /// </summary>
        [LocalizedRequired]
        public string EnglishName { get; set; }

        public static LanguageDto FromLanguage(Language language)
        {
            return SimpleMapper.Map(language, new LanguageDto());
        }
    }
}
