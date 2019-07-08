
namespace Squidex.ICIS.Actions.Kafka
{
    using System.Threading;
    using Avro.Specific;
    using Confluent.Kafka;
    using Confluent.SchemaRegistry;
    using Confluent.Kafka.SyncOverAsync;
    using Confluent.SchemaRegistry.Serdes;

    public class KafkaConsumer<T> : IKafkaConsumer<T> where T : ISpecificRecord
    {
        private readonly CachedSchemaRegistryClient schemaRegistry;
        private readonly IConsumer<string, T> consumer;

        public KafkaConsumer(ConsumerConfig consumerConfig, SchemaRegistryConfig schemaRegistryConfig, string topicName)
        {
            schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

            consumer = new ConsumerBuilder<string, T>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(new AvroDeserializer<T>(schemaRegistry).AsSyncOverAsync())
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
