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
    public class CommentaryTypeMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(EnrichedContentEvent contentEvent)
        {
            var commentaryType = new CommentaryType();

            if (!contentEvent.Data.TryGetValue("id", out var idData))
            {
                throw new System.Exception("Unable to find Id field.");
            }

            commentaryType.Id = idData["iv"].ToString();

            if (!contentEvent.Data.TryGetValue("name", out var nameData))
            {
                throw new System.Exception("Unable to find Name field.");
            }

            commentaryType.Name = nameData["iv"].ToString();
            return commentaryType;
        }
    }
}
