// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppLanguagesDto : Resource
    {
        /// <summary>
        /// The languages.
        /// </summary>
        [Required]
        public AppLanguageDto[] Items { get; set; }

        public static AppLanguagesDto FromApp(IAppEntity app, ApiController controller)
        {
            var result = new AppLanguagesDto
            {
                Items = app.LanguagesConfig.OfType<LanguageConfig>()
                    .Select(x => AppLanguageDto.FromLanguage(x, app, controller))
                    .OrderByDescending(x => x.IsMaster)
                    .ThenBy(x => x.Iso2Code)
                    .ToArray()
            };

            return result.CreateLinks(controller, app.Name);
        }

        private AppLanguagesDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppLanguagesController>(x => nameof(x.GetLanguages), values));

            if (controller.HasPermission(Permissions.AppLanguagesCreate, app))
            {
                AddPostLink("create", controller.Url<AppLanguagesController>(x => nameof(x.PostLanguage), values));
            }

            return this;
        }
    }
}
