using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;

namespace Squidex.ICIS.Kafka.Producer
{
    public sealed class KafkaAvroProducer<T> : KafkaProducer<T> where T : ISpecificRecord
    {
        private readonly ISchemaRegistryClient schemaRegistry;

        public KafkaAvroProducer(IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<T>> log, ISchemaRegistryClient schemaRegistry)
            : base(options, log)
        {
            this.schemaRegistry = schemaRegistry;
        }

        protected override void Configure(ProducerBuilder<string, T> builder)
        {
            builder.SetValueSerializer(new AvroSerializer<T>(schemaRegistry));
        }
    }
}
