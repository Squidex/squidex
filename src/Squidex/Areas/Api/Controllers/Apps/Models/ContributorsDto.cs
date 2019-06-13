// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorsDto : Resource
    {
        /// <summary>
        /// The contributors.
        /// </summary>
        [Required]
        public ContributorDto[] Contributors { get; set; }

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
            var contributors = app.Contributors.ToArray(x => ContributorDto.FromIdAndRole(x.Key, x.Value, controller, app.Name));

            var result = new ContributorsDto
            {
                Contributors = contributors,
            };

            if (isInvited)
            {
                result.Metadata = new ContributorsMetadata
                {
                    IsInvited = isInvited.ToString()
                };
            }

            result.MaxContributors = plans.GetPlanForApp(app).MaxContributors;

            return CreateLinks(result, controller, app.Name);
        }

        private static ContributorsDto CreateLinks(ContributorsDto result, ApiController controller, string app)
        {
            return result;
        }
    }
}
