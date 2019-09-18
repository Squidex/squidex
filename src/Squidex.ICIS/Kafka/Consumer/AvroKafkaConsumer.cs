using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using System;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class AvroKafkaConsumer<T> : KafkaConsumer<T>
    {
        public AvroKafkaConsumer(
            IOptions<ICISKafkaOptions> options,
            IOptions<AvroConsumerOptions> consumerOptions,
            ILogger<KafkaConsumer<T>> log, ISchemaRegistryClient schemaRegistry)
            : base(GetTopicName(consumerOptions), options.Value.Consumer, consumerOptions.Value, log, Configure(schemaRegistry))
        {
        }

        private static string GetTopicName(IOptions<AvroConsumerOptions> consumerOptions)
        {
            return consumerOptions.Value.TopicName;
        }

        private static Action<ConsumerBuilder<string, T>> Configure(ISchemaRegistryClient schemaRegistry)
        {
            return builder =>
            {
                builder.SetValueDeserializer(new AvroDeserializer<T>(schemaRegistry).AsSyncOverAsync());
            };
        }
    }
}
