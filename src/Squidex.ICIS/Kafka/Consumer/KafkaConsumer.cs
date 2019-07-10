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
        private readonly IConsumer<string, T> consumer;

        public KafkaConsumer(IOptions<ICISKafkaOptions> options, ConsumerOptions consumerOptions, ISchemaRegistryClient schemaRegistry, ILogger<KafkaConsumer<T>> log)
        {
            var topicName = consumerOptions.SchemaName;

            if (consumerOptions.TopicName != null)
            {
                topicName = consumerOptions.TopicName;
            }

            var config = new ConsumerConfig(options.Value.Consumer)
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
            consumer.Dispose();
        }
    }
}
