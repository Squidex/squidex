// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppLanguagesDto : Resource
    {
        /// <summary>
        /// The languages.
        /// </summary>
        [LocalizedRequired]
        public AppLanguageDto[] Items { get; set; }

        public static AppLanguagesDto FromApp(IAppEntity app, Resources resources)
        {
            var config = app.Languages;

            var result = new AppLanguagesDto
            {
                Items = config.Languages
                    .Select(x => AppLanguageDto.FromLanguage(x.Key, x.Value, config))
                    .Select(x => x.CreateLinks(resources, app))
                    .OrderByDescending(x => x.IsMaster).ThenBy(x => x.Iso2Code)
                    .ToArray()
            };

            return result.CreateLinks(resources);
        }

        private AppLanguagesDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));

            if (resources.CanCreateLanguage)
            {
                AddPostLink("create", resources.Url<AppLanguagesController>(x => nameof(x.PostLanguage), values));
            }

            return this;
        }
    }
}
