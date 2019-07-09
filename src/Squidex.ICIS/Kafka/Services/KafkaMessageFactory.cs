// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Avro.Specific;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;

namespace Squidex.ICIS.Kafka.Services
{
    public static class KafkaMessageFactory
    {
        public static ISpecificRecord GetKafkaMessage(string schemaName, EnrichedContentEvent contentEvent, IAppEntity commentaryApp, IContentRepository contentRepository)
        {
            ISpecificRecord entity = null;
            switch (schemaName)
            {
                case "commentary":
                    entity = new CommentaryMapper(commentaryApp, contentRepository).ToAvro(contentEvent);
                    break;
                case "commentary-type":
                    entity = new CommentaryTypeMapper().ToAvro(contentEvent);
                    break;
                case "commodity":
                    entity = new CommodityMapper().ToAvro(contentEvent);
                    break;
                case "region":
                    entity = new RegionMapper().ToAvro(contentEvent);
                    break;
                default:
                    throw new Exception($"Schema {schemaName} not configured for Kafka Integration.");
            }

            return entity;
        }
    }
}
