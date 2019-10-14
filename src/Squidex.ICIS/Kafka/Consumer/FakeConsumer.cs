using Confluent.Kafka;
using Squidex.ICIS.Kafka.Entities;
using System;
using System.Threading;

namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class FakeConsumer : IKafkaConsumer<IRefDataEntity>
    {
        private readonly int itemCount;
        private readonly IRefDataEntity source;
        private int item;

        public FakeConsumer(int itemCount, Type type)
        {
            this.itemCount = itemCount;

            source = (IRefDataEntity)Activator.CreateInstance(type);
        }

        public ConsumeResult<string, IRefDataEntity> Consume(CancellationToken cancellationToken)
        {
            if (item == itemCount)
            {
                Thread.Sleep(Timeout.Infinite);
            }

            item++;

            var value = source.CreateFake(item);

            return new ConsumeResult<string, IRefDataEntity>
            {
                Message = new Message<string, IRefDataEntity>
                {
                    Key = item.ToString(), Value = value
                }
            };
        }

        public void Dispose()
        {
        }
    }
}
