using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Kafka.Entities;
using System;

namespace Squidex.ICIS.Kafka.Producer
{
    public sealed class KafkaJsonProducer : KafkaProducer<IRefDataEntity>
    {
        public KafkaJsonProducer(Type type, IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<IRefDataEntity>> log) 
            : base(options, log, Configure(type))
        {
        }

        private static Action<ProducerBuilder<string, IRefDataEntity>> Configure(Type type)
        {
            return builder =>
            {
                builder.SetValueSerializer(new KafkaJsonSerializer<IRefDataEntity>(type));
            };
        }
    }
}
