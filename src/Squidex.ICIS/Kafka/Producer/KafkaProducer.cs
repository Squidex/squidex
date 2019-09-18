// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Config;
using Squidex.ICIS.Utilities;

namespace Squidex.ICIS.Kafka.Producer
{
    public abstract class KafkaProducer<T> : IKafkaProducer<T>
    {
        private readonly IProducer<string, T> producer;

        protected KafkaProducer(IOptions<ICISKafkaOptions> options, ILogger<KafkaProducer<T>> log)
        {
            var builder = 
                new ProducerBuilder<string, T>(options.Value.Producer)
                    .SetKeySerializer(Serializers.Utf8)
                    .SetLogHandler(LogFactory<T>.ProducerLog(log))
                    .SetErrorHandler(LogFactory<T>.ProducerError(log))
                    .SetStatisticsHandler(LogFactory<T>.ProducerStats(log));

            Configure(builder);

            producer = builder.Build();
        }

        protected virtual void Configure(ProducerBuilder<string, T> builder)
        {
        }

        public async Task<DeliveryResult<string, T>> Send(string topicName, string key, T val)
        {
            var message = new Message<string, T>
            {
                Key = key,
                Value = val,
            };

            return await producer.ProduceAsync(topicName, message);
        }

        public void Dispose()
        {
            producer?.Dispose();
        }
    }
}