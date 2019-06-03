// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Repositories;

namespace Squidex.Extensions.Actions.Kafka
{
    public static class KafkaMessageFactory
    {
        public static ISpecificRecord GetKafkaMessage(string topicName, EnrichedContentEvent contentEvent)
        {
            ISpecificRecord entity = null;
            switch (topicName)
            {
                case "Commentary":
                    entity = new CommentaryMapper().ToAvro(contentEvent);
                    break;
                case "CommentaryType":
                    entity = new CommentaryTypeMapper().ToAvro(contentEvent);
                    break;
                default:
                    throw new Exception("kafka Topic not configured.");
            }

            return entity;
        }
    }
}
