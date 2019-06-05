// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Extensions.Actions.Kafka.Entities;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Extensions.Actions.Kafka
{
    public class CommentaryMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(EnrichedContentEvent contentEvent)
        {
            var commentary = new Commentary();
            commentary.Id = contentEvent.Id.ToString();
            commentary.LastModified = contentEvent.LastModified.ToUnixTimeSeconds();

            if (!contentEvent.Data.TryGetValue("Body", out var bodyData))
            {
                throw new System.Exception("Unable to find Body field.");
            }

            commentary.Body = bodyData["en"]?.ToString();

            if (!contentEvent.Data.TryGetValue("CommentaryType", out var commentaryTypeData))
            {
                throw new System.Exception("Unable to find CommentaryType field.");
            }

            if (!contentEvent.Data.TryGetValue("Commodity", out var commodityData))
            {
                throw new System.Exception("Unable to find Commodity field.");
            }

            commentary.CommentaryTypeId = ((Collection<IJsonValue>)commentaryTypeData["iv"])[0].ToString();
            commentary.CommodityId = ((Collection<IJsonValue>)commodityData["iv"])[0].ToString();

            return commentary;
        }
    }
}
