// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro.Specific;

namespace Squidex.Extensions.Actions.Kafka
{
    public static class KafkaProducerFactory
    {
        public static IKafkaProducer<ISpecificRecord> GetKafkaProducer(string topicName, string brokerUrl, string schemaRegistryUrl)
        {
            IKafkaProducer<ISpecificRecord> kafkaProducer = null;
            switch (topicName)
            {
                case "Commentary":
                    kafkaProducer = (IKafkaProducer<ISpecificRecord>)new KafkaCommentaryProducer(topicName, brokerUrl, schemaRegistryUrl);
                    break;
                case "CommentaryType":
                    kafkaProducer = (IKafkaProducer<ISpecificRecord>)new KafkaCommentaryTypeProducer(topicName, brokerUrl, schemaRegistryUrl);
                    break;
                default:
                    break;
            }

            return kafkaProducer;
        }
    }
}
