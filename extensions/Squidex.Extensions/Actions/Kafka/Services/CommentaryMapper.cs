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
        public ISpecificRecord ToAvro(NamedContentData namedContentData)
        {
            var commentary = new Commentary();

            if (!namedContentData.TryGetValue("Body", out var bodyData))
            {
                throw new System.Exception("Unable to find Body field.");
            }

            commentary.Body = bodyData["en"].ToString();

            if (!namedContentData.TryGetValue("CommentaryType", out var commentaryTypeData))
            {
                throw new System.Exception("Unable to find CommentaryType field.");
            }

            if (!namedContentData.TryGetValue("Commodity", out var commodityData))
            {
                throw new System.Exception("Unable to find Commodity field.");
            }

            commentary.CommentaryTypeId = ((Collection<IJsonValue>)commentaryTypeData["iv"])[0].ToString();
            commentary.CommodityId = ((Collection<IJsonValue>)commodityData["iv"])[0].ToString();

            return commentary;
        }
    }
}
