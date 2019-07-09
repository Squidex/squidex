using Squidex.ICIS.Kafka.Consumer;

namespace Squidex.ICIS.Actions.Kafka
{
    using System.Threading;
    using System.Reflection;
    using Avro.Specific;
    using Confluent.Kafka;
    using Confluent.SchemaRegistry;
    using Confluent.Kafka.SyncOverAsync;
    using Confluent.SchemaRegistry.Serdes;
    using Microsoft.Extensions.Logging;
    using Squidex.ICIS.Kafka.Entities;
    using Microsoft.Extensions.Options;
    using Utilities;

    public class KafkaConsumer<T> : IKafkaConsumer<T>
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IConsumer<string, T> consumer;

        public KafkaConsumer(IOptions<ICISKafkaOptions> options, IOptions<CommodityConsumerOptions> commodityConsumerOptions, ILogger<KafkaConsumer<T>> log)
        {
            schemaRegistry = new CachedSchemaRegistryClient(options.Value.SchemaRegistry);

            var topicName = commodityConsumerOptions.Value.SchemaName;

            consumer = new ConsumerBuilder<string, T>(options.Value.Consumer)
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
