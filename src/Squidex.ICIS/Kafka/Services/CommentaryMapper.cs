// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.ICIS.Kafka.Entities;

namespace Squidex.ICIS.Kafka.Services
{
    public static class CommentaryMapper
    {
        private static readonly Status[] PublishedOrDraft = new Status[] { Status.Published, Status.Draft };

        public static async Task<Commentary> ToAvroAsync(EnrichedContentEvent contentEvent, IAppEntity commentaryApp, IContentRepository contentRepository)
        {
            var data = contentEvent.Data;

            var commentaryTypeDbId = data.GetFirstReference("commentarytype");
            var commodityDbId = data.GetFirstReference("commodity");
            var regionDbId = data.GetFirstReference("region");

            var referenced = await contentRepository.QueryAsync(commentaryApp, PublishedOrDraft, new HashSet<Guid> { commentaryTypeDbId, commodityDbId, regionDbId }, true);

            var commentaryType = referenced.Find(x => x.Content.Id == commentaryTypeDbId).Content;
            var commentaryTypeId = commentaryType?.Data.GetString("id");

            var commodity = referenced.Find(x => x.Content.Id == commodityDbId).Content;
            var commodityId = commodity?.Data.GetString("id");

            var region = referenced.Find(x => x.Content.Id == regionDbId).Content;
            var regionId = region?.Data.GetString("id");

            if (commentaryTypeId == null)
            {
                throw new InvalidOperationException("Unable to resolve commentaryType.");
            }

            if (commodityId == null)
            {
                throw new InvalidOperationException("Unable to resolve commodity.");
            }

            if (region == null)
            {
                throw new InvalidOperationException("Unable to resolve region.");
            }

            var commentary = new Commentary
            {
                Id = contentEvent.Id.ToString(),
                LastModified = contentEvent.LastModified.ToUnixTimeSeconds(),
                Body = data.GetString("body", "en"),
                CreatedFor = data.GetTimestamp("createdfor"),
                CommentaryTypeId = commentaryTypeId,
                CommodityId = commodityId,
                RegionId = regionId
            };

            return commentary;
        }
    }
}
