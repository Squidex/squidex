// ==========================================================================
//  CompoundEventConsumerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class CompoundEventConsumerTests
    {
        private readonly IEventConsumer consumer1 = A.Fake<IEventConsumer>();
        private readonly IEventConsumer consumer2 = A.Fake<IEventConsumer>();

        private sealed class MyEvent : IEvent
        {
        }

        [Fact]
        public void Should_return_given_name()
        {
            var sut = new CompoundEventConsumer("consumer-name", consumer1);

            Assert.Equal("consumer-name", sut.Name);
        }

        [Fact]
        public void Should_return_first_inner_name()
        {
            A.CallTo(() => consumer1.Name).Returns("my-inner-consumer");

            var sut = new CompoundEventConsumer(consumer1, consumer2);

            Assert.Equal("my-inner-consumer", sut.Name);
        }

        [Fact]
        public void Should_return_compound_filter()
        {
            A.CallTo(() => consumer1.EventsFilter).Returns("filter1");
            A.CallTo(() => consumer2.EventsFilter).Returns("filter2");

            var sut = new CompoundEventConsumer("my", consumer1, consumer2);

            Assert.Equal("(filter1)|(filter2)", sut.EventsFilter);
        }

        [Fact]
        public void Should_ignore_empty_filters()
        {
            A.CallTo(() => consumer1.EventsFilter).Returns("filter1");
            A.CallTo(() => consumer2.EventsFilter).Returns("");

            var sut = new CompoundEventConsumer("my", consumer1, consumer2);

            Assert.Equal("(filter1)", sut.EventsFilter);
        }

        [Fact]
        public async Task Should_clear_all_consumers()
        {
            A.CallTo(() => consumer1.ClearAsync()).
                Returns(TaskHelper.Done);

            A.CallTo(() => consumer2.ClearAsync())
                .Returns(TaskHelper.Done);

            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            await sut.ClearAsync();

            A.CallTo(() => consumer1.ClearAsync()).MustHaveHappened();
            A.CallTo(() => consumer2.ClearAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_all_consumers()
        {
            var @event = Envelope.Create(new MyEvent());

            A.CallTo(() => consumer1.On(@event))
                .Returns(TaskHelper.Done);

            A.CallTo(() => consumer2.On(@event))
                .Returns(TaskHelper.Done);

            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            await sut.On(@event);

            A.CallTo(() => consumer1.On(@event)).MustHaveHappened();
            A.CallTo(() => consumer2.On(@event)).MustHaveHappened();
        }
    }
}
