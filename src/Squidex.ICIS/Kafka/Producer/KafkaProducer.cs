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

namespace Squidex.ICIS.Actions.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T> where T : ISpecificRecord
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IProducer<string, T> producer;

        public KafkaProducer(ProducerConfig producerConfig, SchemaRegistryConfig schemaRegistryConfig)
        {
            schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

            producer = new ProducerBuilder<string, T>(producerConfig)
                .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
                .SetValueSerializer(new AvroSerializer<T>(schemaRegistry))
                .Build();
        }

        public async Task<DeliveryResult<string, T>> Send(string topicName, string key, T val)
        {
            var message = new Message<string, T>
            {
                Key = key,
                Value = val,
            };

            return await producer.ProduceAsync(topicName, message);
        }

        public void Dispose()
        {
            schemaRegistry?.Dispose();
            producer?.Dispose();
        }
    }
}