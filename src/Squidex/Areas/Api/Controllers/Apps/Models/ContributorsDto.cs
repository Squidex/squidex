// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorsDto : Resource
    {
        /// <summary>
        /// The contributors.
        /// </summary>
        [Required]
        public ContributorDto[] Items { get; set; }

        /// <summary>
        /// The maximum number of allowed contributors.
        /// </summary>
        public int MaxContributors { get; set; }

        /// <summary>
        /// The metadata.
        /// </summary>
        [JsonProperty("_meta")]
        public ContributorsMetadata Metadata { get; set; }

        public static ContributorsDto FromApp(IAppEntity app, IAppPlansProvider plans, ApiController controller, bool isInvited)
        {
            var result = new ContributorsDto
            {
                Items = app.Contributors.Select(x => ContributorDto.FromIdAndRole(x.Key, x.Value, controller, app.Name)).ToArray(),
            };

            if (isInvited)
            {
                result.Metadata = new ContributorsMetadata
                {
                    IsInvited = isInvited.ToString()
                };
            }

            result.MaxContributors = plans.GetPlanForApp(app).MaxContributors;

            return result.CreateLinks(controller, app.Name);
        }

        private ContributorsDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<AppContributorsController>(x => nameof(x.GetContributors), values));

            if (controller.HasPermission(Permissions.AppContributorsAssign, app) && (MaxContributors < 0 || Items.Length < MaxContributors))
            {
                AddPostLink("create", controller.Url<AppContributorsController>(x => nameof(x.PostContributor), values));
            }

            return this;
        }
    }
}
