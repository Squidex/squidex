// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppLanguageDto : Resource
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
        public Language[] Fallback { get; set; }

        /// <summary>
        /// Indicates if the language is the master language.
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// Indicates if the language is optional.
        /// </summary>
        public bool IsOptional { get; set; }

        public static AppLanguageDto FromLanguage(LanguageConfig language, IAppEntity app, ApiController controller)
        {
            var result = SimpleMapper.Map(language.Language,
                new AppLanguageDto
                {
                    IsMaster = language == app.LanguagesConfig.Master,
                    IsOptional = language.IsOptional,
                    Fallback = language.LanguageFallbacks.ToArray()
                });

            return result.CreateLinks(controller, app);
        }

        private AppLanguageDto CreateLinks(ApiController controller, IAppEntity app)
        {
            var values = new { app = app.Name, language = Iso2Code };

            if (!IsMaster)
            {
                if (controller.HasPermission(Permissions.AppLanguagesUpdate, app.Name))
                {
                    AddPutLink("update", controller.Url<AppLanguagesController>(x => nameof(x.PutLanguage), values));
                }

                if (controller.HasPermission(Permissions.AppLanguagesDelete, app.Name) && app.LanguagesConfig.Count > 1)
                {
                    AddDeleteLink("delete", controller.Url<AppLanguagesController>(x => nameof(x.DeleteLanguage), values));
                }
            }

            return this;
        }
    }
}
