﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class CompoundEventConsumerTests
    {
        private readonly IEventConsumer consumer1 = A.Fake<IEventConsumer>();
        private readonly IEventConsumer consumer2 = A.Fake<IEventConsumer>();

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
        public void Should_return_compound_filter_from_array()
        {
            A.CallTo(() => consumer1.EventsFilter).Returns("filter1");
            A.CallTo(() => consumer2.EventsFilter).Returns("filter2");

            var sut = new CompoundEventConsumer(new[] { consumer1, consumer2 });

            Assert.Equal("(filter1)|(filter2)", sut.EventsFilter);
        }

        [Fact]
        public void Should_ignore_empty_filters()
        {
            A.CallTo(() => consumer1.EventsFilter).Returns("filter1");
            A.CallTo(() => consumer2.EventsFilter).Returns(string.Empty);

            var sut = new CompoundEventConsumer("my", consumer1, consumer2);

            Assert.Equal("(filter1)", sut.EventsFilter);
        }

        [Fact]
        public async Task Should_clear_all_consumers()
        {
            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            await sut.ClearAsync();

            A.CallTo(() => consumer1.ClearAsync()).MustHaveHappened();
            A.CallTo(() => consumer2.ClearAsync()).MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_all_consumers()
        {
            var @event = Envelope.Create<IEvent>(new MyEvent());

            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            await sut.On(@event);

            A.CallTo(() => consumer1.On(@event)).MustHaveHappened();
            A.CallTo(() => consumer2.On(@event)).MustHaveHappened();
        }

        [Fact]
        public void Should_handle_if_any_consumer_handles()
        {
            var stored = new StoredEvent("Stream", "1", 1, new EventData("Type", new EnvelopeHeaders(), "Payload"));

            A.CallTo(() => consumer1.Handles(stored))
                .Returns(false);

            A.CallTo(() => consumer2.Handles(stored))
                .Returns(true);

            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            var result = sut.Handles(stored);

            Assert.True(result);
        }

        [Fact]
        public void Should_no_handle_if_no_consumer_handles()
        {
            var stored = new StoredEvent("Stream", "1", 1, new EventData("Type", new EnvelopeHeaders(), "Payload"));

            A.CallTo(() => consumer1.Handles(stored))
                .Returns(false);

            A.CallTo(() => consumer2.Handles(stored))
                .Returns(false);

            var sut = new CompoundEventConsumer("consumer-name", consumer1, consumer2);

            var result = sut.Handles(stored);

            Assert.False(result);
        }
    }
}
