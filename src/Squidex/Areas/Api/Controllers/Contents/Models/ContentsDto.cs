// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
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
        /// All available statuses.
        /// </summary>
        [Required]
        public string[] Statuses { get; set; }

        public string ToEtag()
        {
            return Items.ToManyEtag(Total);
        }

        public string ToSurrogateKeys()
        {
            return Items.ToSurrogateKeys();
        }

        public static ContentsDto FromContents(IList<IContentEntity> contents, QueryContext context, ApiController controller, string app, string schema)
        {
            var result = new ContentsDto
            {
                Total = contents.Count,
                Items = contents.Select(x => ContentDto.FromContent(x, context, controller, app, schema)).ToArray()
            };

            return result.CreateLinks(controller, app, schema);
        }

        public static ContentsDto FromContents(IResultList<IContentEntity> contents, QueryContext context, ApiController controller, string app, string schema)
        {
            var result = new ContentsDto
            {
                Total = contents.Total,
                Items = contents.Select(x => ContentDto.FromContent(x, context, controller, app, schema)).ToArray(),
                Statuses = new[] { "Published", "Draft", "Archived" }
            };

            return result.CreateLinks(controller, app, schema);
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
