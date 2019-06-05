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

namespace Squidex.Extensions.Actions.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T> where T : ISpecificRecord
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly string topicName;
        private readonly IProducer<string, T> producer;

        public KafkaProducer(string topicName, string brokerUrl, string schemaRegistryUrl)
        {
            this.topicName = topicName;
            schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
            {
                SchemaRegistryUrl = schemaRegistryUrl,
                SchemaRegistryRequestTimeoutMs = 5000,
                SchemaRegistryMaxCachedSchemas = 10
            });

            var config = new ProducerConfig
            {
                BootstrapServers = brokerUrl,
                Partitioner = Partitioner.Murmur2Random,
            };

            producer = new ProducerBuilder<string, T>(config)
                .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
                .SetValueSerializer(new AvroSerializer<T>(schemaRegistry))
                .Build();
        }

        public async Task<DeliveryResult<string, T>> Send(string key, T val)
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