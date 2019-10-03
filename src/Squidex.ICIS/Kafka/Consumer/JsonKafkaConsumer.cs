using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Kafka.Entities;
using System;
using System.Reflection;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class KafkaJsonConsumer : KafkaConsumer<IRefDataEntity>
    {
        public KafkaJsonConsumer(Type type, IOptions<ICISKafkaOptions> options, IOptions<ConsumerOptions> consumerOptions, ILogger<KafkaConsumer<IRefDataEntity>> log)
            : base(GetTopicName(type, consumerOptions.Value), options.Value.Consumer, consumerOptions.Value, log, Configure(type))
        {
        }

        private static string GetTopicName(Type type, ConsumerOptions consumerOptions)
        {
            return type.GetCustomAttribute<TopicNameAttribute>().GetName(consumerOptions);
        }

        private static Action<ConsumerBuilder<string, IRefDataEntity>> Configure(Type type)
        {
            return builder =>
            {
                builder.SetValueDeserializer(new KafkaJsonSerializer<IRefDataEntity>(type));
            };
        }
    }
}
