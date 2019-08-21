// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.ICIS.Kafka.Entities;

namespace Squidex.ICIS.Kafka.Services
{
    public class CommodityMapper
    {
        public static Commodity ToAvro(EnrichedContentEvent contentEvent)
        {
            var data = contentEvent.Data;

            var commentaryType = new Commodity
            {
                Id = data.GetString("id"),
                Name = data.GetString("name")
            };

            return commentaryType;
        }
    }
}
