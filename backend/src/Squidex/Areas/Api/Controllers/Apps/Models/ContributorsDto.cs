// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class ContributorsDto : Resource
    {
        /// <summary>
        /// The contributors.
        /// </summary>
        [LocalizedRequired]
        public ContributorDto[] Items { get; set; }

        /// <summary>
        /// The maximum number of allowed contributors.
        /// </summary>
        public int MaxContributors { get; set; }

        /// <summary>
        /// The metadata to provide information about this request.
        /// </summary>
        [JsonProperty("_meta")]
        public ContributorsMetadata Metadata { get; set; }

        public static async Task<ContributorsDto> FromAppAsync(IAppEntity app, Resources resources, IUserResolver userResolver, IAppPlansProvider plans, bool invited)
        {
            var users = await userResolver.QueryManyAsync(app.Contributors.Keys.ToArray());

            var result = new ContributorsDto
            {
                Items =
                    app.Contributors
                        .Select(x => ContributorDto.FromIdAndRole(x.Key, x.Value))
                        .Select(x => x.WithUser(users))
                        .Select(x => x.WithLinks(resources))
                        .OrderBy(x => x.ContributorName)
                        .ToArray()
            };

            result.WithInvited(invited);
            result.WithPlan(app, plans);

            return result.CreateLinks(resources, app.Name);
        }

        private void WithPlan(IAppEntity app, IAppPlansProvider plans)
        {
            MaxContributors = plans.GetPlanForApp(app).Plan.MaxContributors;
        }

        private void WithInvited(bool isInvited)
        {
            if (isInvited)
            {
                Metadata = new ContributorsMetadata
                {
                    IsInvited = "true"
                };
            }
        }

        private ContributorsDto CreateLinks(Resources resources, string app)
        {
            var values = new { app };

            AddSelfLink(resources.Url<AppContributorsController>(x => nameof(x.GetContributors), values));

            if (resources.CanAssignContributor && (MaxContributors < 0 || Items.Length < MaxContributors))
            {
                AddPostLink("create", resources.Url<AppContributorsController>(x => nameof(x.PostContributor), values));
            }

            return this;
        }
    }
}
