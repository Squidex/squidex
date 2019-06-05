﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Extensions.Actions.Kafka.Entities;

namespace Squidex.Extensions.Actions.Kafka
{
    public class CommentaryTypeMapper : IKafkaMessageMapper
    {
        public ISpecificRecord ToAvro(EnrichedContentEvent contentEvent)
        {
            var commentaryType = new CommentaryType();
            commentaryType.LastModified = contentEvent.LastModified.ToUnixTimeSeconds();

            if (!contentEvent.Data.TryGetValue("Id", out var idData))
            {
                throw new System.Exception("Unable to find Id field.");
            }

            commentaryType.Id = idData["iv"].ToString();

            if (!contentEvent.Data.TryGetValue("Name", out var nameData))
            {
                throw new System.Exception("Unable to find Name field.");
            }

            commentaryType.Name = nameData["iv"].ToString();
            return commentaryType;
        }
    }
}
