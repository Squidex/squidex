// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Repositories;

namespace Squidex.Extensions.Actions.Kafka
{
    public static class KafkaMessageFactory
    {
        public static ISpecificRecord GetKafkaMessage(string topicName, NamedContentData namedContentData)
        {
            ISpecificRecord entity = null;
            switch (topicName)
            {
                case "Commentary":
                    entity = new CommentaryMapper().ToAvro(namedContentData);
                    break;
                case "CommentaryType":
                    entity = new CommentaryTypeMapper().ToAvro(namedContentData);
                    break;
                default:
                    throw new Exception("kafka Topic not configured.");
            }

            return entity;
        }
    }
}
