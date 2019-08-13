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
            var data = contentEvent.Data;

            var commentaryType = new Region
            {
                Id = data.GetInvariantString("id"),
                Name = data.GetInvariantString("name")
            };

            return commentaryType;
        }
    }
}
