// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Shared;
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
        [Required]
        public ContentDto[] Items { get; set; }

        /// <summary>
        /// The possible statuses.
        /// </summary>
        [Required]
        public StatusInfoDto[] Statuses { get; set; }

        public static async Task<ContentsDto> FromContentsAsync(IResultList<IEnrichedContentEntity> contents,
            Context context, ApiController controller, ISchemaEntity schema, IContentWorkflow contentWorkflow)
        {
            var result = new ContentsDto
            {
                Total = contents.Total,
                Items = contents.Select(x => ContentDto.FromContent(context, x, controller)).ToArray()
            };

            await result.AssignStatusesAsync(contentWorkflow, schema);

            return result.CreateLinks(controller, schema.AppId.Name, schema.SchemaDef.Name);
        }

        private async Task AssignStatusesAsync(IContentWorkflow contentWorkflow, ISchemaEntity schema)
        {
            var allStatuses = await contentWorkflow.GetAllAsync(schema);

            Statuses = allStatuses.Select(StatusInfoDto.FromStatusInfo).ToArray();
        }

        private ContentsDto CreateLinks(ApiController controller, string app, string schema)
        {
            if (schema != null)
            {
                var values = new { app, name = schema };

                AddSelfLink(controller.Url<ContentsController>(x => nameof(x.GetContents), values));

                if (controller.HasPermission(Permissions.AppContentsCreate, app, schema))
                {
                    AddPostLink("create", controller.Url<ContentsController>(x => nameof(x.PostContent), values));

                    AddPostLink("create/publish", controller.Url<ContentsController>(x => nameof(x.PostContent), values) + "?publish=true");
                }
            }

            return this;
        }
    }
}
