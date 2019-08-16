using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Squidex.Extensions.SelfHosted.Kafka
{
    public interface IKafkaProducer : IDisposable
    {
        Task<DeliveryResult<string, string>> Send(string topicName, string key, string val);
    }
}
