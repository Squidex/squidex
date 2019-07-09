// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro.Specific;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.ICIS.Kafka.Entities;

namespace Squidex.ICIS.Kafka.Services
{
    public class RegionMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(EnrichedContentEvent contentEvent)
        {
            var region = new Region();

            if (!contentEvent.Data.TryGetValue("ID", out var idData))
            {
                throw new System.Exception("Unable to find Id field.");
            }

            region.Id = idData["iv"].ToString();

            if (!contentEvent.Data.TryGetValue("Name", out var nameData))
            {
                throw new System.Exception("Unable to find Name field.");
            }

            region.Name = nameData["iv"].ToString();
            return region;
        }
    }
}
