using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Extensions.SelfHosted.Kafka
{
    public class KafkaProducerOptions : ProducerConfig
    {
        public bool IsProducerConfigured()
        {
            return !string.IsNullOrWhiteSpace(this.BootstrapServers);
        }
    }
}
