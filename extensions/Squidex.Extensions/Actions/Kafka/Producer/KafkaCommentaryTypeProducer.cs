// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Newtonsoft.Json;
using Squidex.Extensions.Actions.Kafka.Entities;

namespace Squidex.Extensions.Actions.Kafka
{
    public class KafkaCommentaryTypeProducer : KafkaProducer<CommentaryType>
    {
        public KafkaCommentaryTypeProducer(string topicName, string brokerUrl, string schemaRegistryUrl)
            : base(topicName, brokerUrl, schemaRegistryUrl)
        {
        }
    }
}