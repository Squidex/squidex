namespace Squidex.ICIS.Actions.Kafka
{
    using System;
    using Confluent.Kafka;
    using System.Threading;
    using Avro.Specific;

    public interface IKafkaConsumer<T> : IDisposable where T : ISpecificRecord
    {
        ConsumeResult<string, T> Consume(CancellationToken cancellationToken);
    }
}
