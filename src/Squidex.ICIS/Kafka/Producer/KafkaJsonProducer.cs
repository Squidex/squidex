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
        private readonly Type type;

        public KafkaJsonProducer(Type type, IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<IRefDataEntity>> log) 
            : base(options, log)
        {
            this.type = type;
        }

        protected override void Configure(ProducerBuilder<string, IRefDataEntity> builder)
        {
            builder.SetValueSerializer(new KafkaJsonSerializer<IRefDataEntity>(type));
        }
    }
}
