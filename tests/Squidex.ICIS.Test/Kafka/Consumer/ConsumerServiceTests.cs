using Confluent.Kafka;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.ICIS.Test.Kafka.Consumer
{
    public class ConsumerServiceTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IKafkaConsumer<string> consumer = A.Fake<IKafkaConsumer<string>>();
        private readonly IKafkaHandler<string> handler = A.Fake<IKafkaHandler<string>>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly ConsumerService<string> sut;

        public ConsumerServiceTests()
        {
            sut = new ConsumerService<string>(
                Options.Create(new ConsumerOptions
                {
                    AppName = "my-app", ClientName = "client"
                }),
                contextProvider,
                consumer,
                handler,
                appProvider,
                log);
        }

        [Fact]
        public async Task Should_forward_message_from_kafka_to_handler()
        {
            var context = new Context();

            var app = A.Fake<IAppEntity>();

            A.CallTo(() => appProvider.GetAppAsync("my-app"))
                .Returns(app);

            A.CallTo(() => contextProvider.Context)
                .Returns(context);

            A.CallTo(() => consumer.Consume(A<CancellationToken>.Ignored))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> {  Key = "key1", Value = "value1" }
                });

            await sut.StartAsync();
            await Task.Delay(1000);
            await sut.StopAsync();

            A.CallTo(() => handler.HandleAsync(A<RefToken>.Ignored, context, "key1", "value1"))
                .MustHaveHappened();

            Assert.Same(app, context.App);
        }

        [Fact]
        public async Task Should_not_forward_message_if_app_not_found()
        {
            var context = new Context();

            A.CallTo(() => appProvider.GetAppAsync("my-app"))
                .Returns(Task.FromResult<IAppEntity>(null));

            A.CallTo(() => contextProvider.Context)
                .Returns(context);

            A.CallTo(() => consumer.Consume(A<CancellationToken>.Ignored))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Key = "key1", Value = "value1" }
                });

            await sut.StartAsync();
            await Task.Delay(1000);
            await sut.StopAsync();

            A.CallTo(() => handler.HandleAsync(A<RefToken>.Ignored, context, "key1", "value1"))
                .MustNotHaveHappened();

            Assert.Null(context.App);
        }
    }
}
