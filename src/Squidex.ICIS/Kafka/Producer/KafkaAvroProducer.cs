using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using System;

namespace Squidex.ICIS.Kafka.Producer
{
    public sealed class KafkaAvroProducer<T> : KafkaProducer<T> where T : ISpecificRecord
    {
        public KafkaAvroProducer(IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<T>> log, ISchemaRegistryClient schemaRegistry)
            : base(options, log, Configure(schemaRegistry))
        {
        }

        private static Action<ProducerBuilder<string, T>> Configure(ISchemaRegistryClient schemaRegistry)
        {
            return builder =>
            {
                builder.SetValueSerializer(new AvroSerializer<T>(schemaRegistry));
            };
        }
    }
}
