using System;
using System.Threading;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Squidex.ICIS.Utilities;

namespace Squidex.ICIS.Kafka.Consumer
{
    public abstract class KafkaConsumer<T> : IKafkaConsumer<T>
    {
        private readonly IConsumer<string, T> consumer;

        protected KafkaConsumer(string topicName,
            ConsumerConfig consumerConfig,
            ConsumerOptions consumerOptions,
            ILogger<KafkaConsumer<T>> log,
            Action<ConsumerBuilder<string, T>> configure = null)
        {
            var config = new ConsumerConfig(consumerConfig)
            {
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            if (string.IsNullOrWhiteSpace(config.GroupId))
            {
                config.GroupId = $"cosmos-{Guid.NewGuid()}";
            }

            if (!string.IsNullOrWhiteSpace(consumerOptions.GroupId))
            {
                config.GroupId = consumerOptions.GroupId;
            }

            var builder =
                new ConsumerBuilder<string, T>(config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetLogHandler(LogFactory<T>.ConsumerLog(log))
                    .SetErrorHandler(LogFactory<T>.ConsumerError(log))
                    .SetStatisticsHandler(LogFactory<T>.ConsumerStats(log));

            configure?.Invoke(builder);

            consumer = builder.Build();
            consumer.Subscribe(topicName);
        }

        public ConsumeResult<string, T> Consume(CancellationToken cancellationToken)
        {
            return consumer.Consume(cancellationToken);
        }

        public void Dispose()
        {
            consumer.Dispose();
        }
    }
}
