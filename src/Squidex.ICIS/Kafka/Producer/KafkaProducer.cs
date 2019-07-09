// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Utilities;

namespace Squidex.ICIS.Kafka.Producer
{
    public class KafkaProducer<T> : IKafkaProducer<T> where T : ISpecificRecord
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IProducer<string, T> producer;

        public KafkaProducer(IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<T>> log)
        {
            schemaRegistry = new CachedSchemaRegistryClient(options.Value.SchemaRegistry);

            producer = new ProducerBuilder<string, T>(options.Value.Producer)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(new AvroSerializer<T>(schemaRegistry))
                .SetLogHandler(LogFactory<T>.ProducerLog(log))
                .SetErrorHandler(LogFactory<T>.ProducerError(log))
                .SetStatisticsHandler(LogFactory<T>.ProducerStats(log))
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