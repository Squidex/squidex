// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AssetScriptsDto : Resource
    {
        /// <summary>
        /// The script that is executed when creating an asset.
        /// </summary>
        public string? Create { get; init; }

        /// <summary>
        /// The script that is executed when updating a content.
        /// </summary>
        public string? Update { get; init; }

        /// <summary>
        /// The script that is executed when annotating a content.
        /// </summary>
        public string? Annotate { get; init; }

        /// <summary>
        /// The script that is executed when moving a content.
        /// </summary>
        public string? Move { get; init; }

        /// <summary>
        /// The script that is executed when deleting a content.
        /// </summary>
        public string? Delete { get; init; }

        /// <summary>
        /// The version of the app.
        /// </summary>
        public long Version { get; set; }

        public static AssetScriptsDto FromApp(IAppEntity app, Resources resources)
        {
            var result = SimpleMapper.Map(app.AssetScripts, new AssetScriptsDto());

            return result.CreateLinks(resources);
        }

        private AssetScriptsDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App };

            AddSelfLink(resources.Url<AppSettingsController>(x => nameof(x.GetSettings), values));

            if (resources.CanUpdateAssetsScripts)
            {
                AddPutLink("update", resources.Url<AppAssetsController>(x => nameof(x.PutScripts), values));
            }

            return this;
        }
    }
}
