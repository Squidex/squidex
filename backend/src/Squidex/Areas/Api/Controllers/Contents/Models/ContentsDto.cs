// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsDto : Resource
    {
        /// <summary>
        /// The total number of content items.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The content items.
        /// </summary>
        [LocalizedRequired]
        public ContentDto[] Items { get; set; }

        /// <summary>
        /// The possible statuses.
        /// </summary>
        [LocalizedRequired]
        public StatusInfoDto[] Statuses { get; set; }

        public static async Task<ContentsDto> FromContentsAsync(IResultList<IEnrichedContentEntity> contents, Resources resources,
            ISchemaEntity? schema, IContentWorkflow workflow)
        {
            var result = new ContentsDto
            {
                Total = contents.Total,
                Items = contents.Select(x => ContentDto.FromContent(x, resources)).ToArray()
            };

            if (schema != null)
            {
                await result.AssignStatusesAsync(workflow, schema);

                result.CreateLinks(resources, schema.SchemaDef.Name);
            }

            return result;
        }

        private async Task AssignStatusesAsync(IContentWorkflow workflow, ISchemaEntity schema)
        {
            var allStatuses = await workflow.GetAllAsync(schema);

            Statuses = allStatuses.Select(StatusInfoDto.FromStatusInfo).ToArray();
        }

        private void CreateLinks(Resources resources, string schema)
        {
            var values = new { app = resources.App, schema };

            AddSelfLink(resources.Url<ContentsController>(x => nameof(x.GetContents), values));

            if (resources.CanCreateContent(schema))
            {
                AddPostLink("create", resources.Url<ContentsController>(x => nameof(x.PostContent), values));

                var publishValues = new { values.app, values.schema, publish = true };

                AddPostLink("create/publish", resources.Url<ContentsController>(x => nameof(x.PostContent), publishValues));
            }
        }
    }
}
