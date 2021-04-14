// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppSettingsDto : Resource
    {
        /// <summary>
        /// The configured app patterns.
        /// </summary>
        [LocalizedRequired]
        public List<PatternDto> Patterns { get; set; }

        /// <summary>
        /// The configured UI editors.
        /// </summary>
        [LocalizedRequired]
        public List<EditorDto> Editors { get; set; }

        /// <summary>
        /// Hide the scheduler for content items.
        /// </summary>
        public bool HideScheduler { get; set; }

        /// <summary>
        /// The version of the app.
        /// </summary>
        public long Version { get; set; }

        public static AppSettingsDto FromApp(IAppEntity app, Resources resources)
        {
            var settings = app.Settings;

            var result = new AppSettingsDto
            {
                HideScheduler = settings.HideScheduler,
                Patterns =
                    settings.Patterns
                        .Select(x => SimpleMapper.Map(x, new PatternDto())).ToList(),
                Editors =
                    settings.Editors
                        .Select(x => SimpleMapper.Map(x, new EditorDto())).ToList(),
                Version = app.Version
            };

            return result.CreateLinks(resources);
        }

        private AppSettingsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppsController>(x => nameof(x.GetAppSettings), values));

            if (resources.CanUpdateSettings)
            {
                AddPutLink("update", resources.Url<AppsController>(x => nameof(x.PutAppSettings), values));
            }

            return this;
        }
    }
}
