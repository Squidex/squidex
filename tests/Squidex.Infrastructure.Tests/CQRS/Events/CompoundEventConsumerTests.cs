// ==========================================================================
//  CompoundEventConsumerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class CompoundEventConsumerTests
    {
        private readonly Mock<IEventConsumer> consumer1 = new Mock<IEventConsumer>();
        private readonly Mock<IEventConsumer> consumer2 = new Mock<IEventConsumer>();

        private sealed class MyEvent : IEvent
        {
        }

        [Fact]
        public void Should_return_given_name()
        {
            var sut = new CompoundEventConsumer("consumer-name");

            Assert.Equal("consumer-name", sut.Name);
        }

        [Fact]
        public void Should_return_first_inner_name()
        {
            var sut = new CompoundEventConsumer(consumer1.Object, consumer2.Object);

            Assert.Equal(consumer1.Object.GetType().Name, sut.Name);
        }

        [Fact]
        public async Task Should_clear_all_consumers()
        {
            consumer1.Setup(x => x.ClearAsync()).Returns(TaskHelper.Done).Verifiable();
            consumer2.Setup(x => x.ClearAsync()).Returns(TaskHelper.Done).Verifiable();

            var sut = new CompoundEventConsumer("consumer-name", consumer1.Object, consumer2.Object);

            await sut.ClearAsync();

            consumer1.VerifyAll();
            consumer2.VerifyAll();
        }

        [Fact]
        public async Task Should_invoke_all_consumers()
        {
            var @event = Envelope.Create(new MyEvent());

            consumer1.Setup(x => x.On(@event)).Returns(TaskHelper.Done).Verifiable();
            consumer2.Setup(x => x.On(@event)).Returns(TaskHelper.Done).Verifiable();

            var sut = new CompoundEventConsumer("consumer-name", consumer1.Object, consumer2.Object);

            await sut.On(@event);

            consumer1.VerifyAll();
            consumer2.VerifyAll();
        }
    }
}
