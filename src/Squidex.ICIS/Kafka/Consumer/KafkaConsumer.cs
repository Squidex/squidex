
namespace Squidex.ICIS.Actions.Kafka
{
    using System.Threading;
    using Avro.Specific;
    using Confluent.Kafka;
    using Confluent.SchemaRegistry;
    using Confluent.Kafka.SyncOverAsync;
    using Confluent.SchemaRegistry.Serdes;
    using Squidex.Infrastructure.Log;

    public class KafkaConsumer<T> : IKafkaConsumer<T> where T : ISpecificRecord
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IConsumer<string, T> consumer;
        private readonly ISemanticLog log;

        public KafkaConsumer(ConsumerConfig consumerConfig, SchemaRegistryConfig schemaRegistryConfig, string topicName, ISemanticLog log)
        {
            schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

            consumer = new ConsumerBuilder<string, T>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(new AvroDeserializer<T>(schemaRegistry).AsSyncOverAsync())
                .SetLogHandler((consumer, message) =>
                {
                    // TODO
                })
                .SetErrorHandler((consumer, message) =>
                {
                    // TODO
                })
                .SetStatisticsHandler((consumer, message) =>
                {
                    // TODO
                })
                .Build();

            consumer.Subscribe(topicName);
            this.log = log;
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
