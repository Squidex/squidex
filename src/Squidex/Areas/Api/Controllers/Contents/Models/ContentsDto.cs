// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
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
        public ContentDto[] Items { get; set; }

        public string ToEtag()
        {
            return Items.ToManyEtag(Total);
        }

        public string ToSurrogateKeys()
        {
            return Items.ToSurrogateKeys();
        }

        public static ContentsDto FromContents(IList<IContentEntity> contents, QueryContext context, ApiController controller, string app)
        {
            var result = new ContentsDto
            {
                Total = contents.Count,
                Items = contents.Select(x => ContentDto.FromContent(x, context, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        public static ContentsDto FromContents(IResultList<IContentEntity> contents, QueryContext context, ApiController controller, string app)
        {
            var result = new ContentsDto
            {
                Total = contents.Total,
                Items = contents.Select(x => ContentDto.FromContent(x, context, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        private ContentsDto CreateLinks(ApiController controller, string app)
        {
            return this;
        }
    }
}
