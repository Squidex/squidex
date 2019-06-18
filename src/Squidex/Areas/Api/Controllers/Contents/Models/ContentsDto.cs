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
        public Status2[] Statuses { get; set; }

        public string ToEtag()
        {
            return Items.ToManyEtag(Total);
        }

        public string ToSurrogateKeys()
        {
            return Items.ToSurrogateKeys();
        }

        public static async Task<ContentsDto> FromContentsAsync(IResultList<IContentEntity> contents, QueryContext context,
            ApiController controller,
            ISchemaEntity schema,
            IContentWorkflow contentWorkflow)
        {
            var result = new ContentsDto
            {
                Total = contents.Total,
                Items = new ContentDto[contents.Count]
            };

            await Task.WhenAll(
                result.AssignContentsAsync(contentWorkflow, contents, context, controller),
                result.AssignStatusesAsync(contentWorkflow, schema));

            return result.CreateLinks(controller, schema.AppId.Name, schema.SchemaDef.Name);
        }

        private async Task AssignStatusesAsync(IContentWorkflow contentWorkflow, ISchemaEntity schema)
        {
            var allStatuses = await contentWorkflow.GetAllAsync(schema);

            Statuses = allStatuses.ToArray();
        }

        private async Task AssignContentsAsync(IContentWorkflow contentWorkflow, IResultList<IContentEntity> contents, QueryContext context, ApiController controller)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                Items[i] = await ContentDto.FromContentAsync(context, contents[i], contentWorkflow, controller);
            }
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

                    if (controller.HasPermission(Helper.StatusPermission(app, schema, Status.Published)))
                    {
                        AddPostLink("create/publish", controller.Url<ContentsController>(x => nameof(x.PostContent), values) + "?publish=true");
                    }
                }
            }

            return this;
        }
    }
}
