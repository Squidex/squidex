using System;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Utilities;

namespace Squidex.ICIS.Kafka.Consumer
{
    public class KafkaConsumer<T> : IKafkaConsumer<T>
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IConsumer<string, T> consumer;

        public KafkaConsumer(IOptions<ICISKafkaOptions> options, ConsumerOptions commodityConsumerOptions, ILogger<KafkaConsumer<T>> log)
        {
            schemaRegistry = new CachedSchemaRegistryClient(options.Value.SchemaRegistry);

            var topicName = commodityConsumerOptions.SchemaName;

            if (commodityConsumerOptions.TopicName != null)
            {
                topicName = commodityConsumerOptions.TopicName;
            }

            var config = new ConsumerConfig(options.Value.Consumer)
            {
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            if (string.IsNullOrWhiteSpace(config.GroupId))
            {
                config.GroupId = $"cosmos-{Guid.NewGuid()}";
            }

            if (!string.IsNullOrWhiteSpace(commodityConsumerOptions.GroupId))
            {
                config.GroupId = commodityConsumerOptions.GroupId;
            }

            consumer = new ConsumerBuilder<string, T>(config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(new AvroDeserializer<T>(schemaRegistry).AsSyncOverAsync())
                .SetLogHandler(LogFactory<T>.ConsumerLog(log))
                .SetErrorHandler(LogFactory<T>.ConsumerError(log))
                .SetStatisticsHandler(LogFactory<T>.ConsumerStats(log))
                .Build();

            consumer.Subscribe(topicName);
        }

        public ConsumeResult<string, T> Consume(CancellationToken cancellationToken)
        {
            return consumer.Consume(cancellationToken);
        }

        public void Dispose()
        {
            schemaRegistry.Dispose();
            consumer.Dispose();
        }
    }
}
