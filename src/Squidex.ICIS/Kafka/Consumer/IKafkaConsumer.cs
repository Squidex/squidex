using System;
using System.Threading;
using Confluent.Kafka;

namespace Squidex.ICIS.Kafka.Consumer
{
    public interface IKafkaConsumer<T> : IDisposable
    {
        ConsumeResult<string, T> Consume(CancellationToken cancellationToken);
    }
}
