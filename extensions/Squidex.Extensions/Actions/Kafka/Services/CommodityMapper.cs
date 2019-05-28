// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Extensions.Actions.Kafka.Entities;

namespace Squidex.Extensions.Actions.Kafka
{
    public class CommodityMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(NamedContentData namedContentData)
        {
            var commentaryType = new Commodity();
            ContentFieldData idData = null;

            if (!namedContentData.TryGetValue("Id", out idData))
            {
                throw new System.Exception("Unable to find Id field.");
            }

            commentaryType.Id = idData["iv"].ToString();
            ContentFieldData nameData = null;

            if (!namedContentData.TryGetValue("Name", out nameData))
            {
                throw new System.Exception("Unable to find Name field.");
            }

            commentaryType.Name = nameData["iv"].ToString();
            return commentaryType;
        }
    }
}
