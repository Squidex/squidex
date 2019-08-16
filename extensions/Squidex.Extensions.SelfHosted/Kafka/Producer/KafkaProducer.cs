using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Squidex.Extensions.SelfHosted.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> producer;

        public KafkaProducer(IOptions<KafkaProducerOptions> options)
        {
            producer = new ProducerBuilder<string, string>(options.Value)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.Utf8)
                .Build();
        }

        public async Task<DeliveryResult<string, string>> Send(string topicName, string key, string val)
        {
            var message = new Message<string, string>
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
