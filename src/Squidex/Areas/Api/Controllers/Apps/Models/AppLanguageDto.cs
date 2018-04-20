// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppLanguageDto
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

        /// <summary>
        /// The fallback languages.
        /// </summary>
        [Required]
        public List<Language> Fallback { get; set; }

        /// <summary>
        /// Indicates if the language is the master language.
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// Indicates if the language is optional.
        /// </summary>
        public bool IsOptional { get; set; }

        public static AppLanguageDto FromCommand(AddLanguage command)
        {
            return SimpleMapper.Map(command.Language, new AppLanguageDto { Fallback = new List<Language>() });
        }

        public static AppLanguageDto[] FromApp(IAppEntity app)
        {
            return app.LanguagesConfig.OfType<LanguageConfig>().Select(x => FromLanguage(x, app)).OrderByDescending(x => x.IsMaster).ThenBy(x => x.Iso2Code).ToArray();
        }

        private static AppLanguageDto FromLanguage(LanguageConfig x, IAppEntity app)
        {
            return SimpleMapper.Map(x.Language,
                new AppLanguageDto
                {
                    IsMaster = x == app.LanguagesConfig.Master,
                    IsOptional = x.IsOptional,
                    Fallback = x.LanguageFallbacks.ToList()
                });
        }
    }
}
