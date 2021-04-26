// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class RetrySubscriptionTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventSubscriber eventSubscriber = A.Fake<IEventSubscriber>();
        private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
        private readonly IEventSubscriber sutSubscriber;
        private readonly RetrySubscription sut;

        public RetrySubscriptionTests()
        {
            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .Returns(eventSubscription);

            A.CallTo(() => eventSubscription.Sender)
                .Returns(eventSubscription);

            sut = new RetrySubscription(eventSubscriber, s => eventStore.CreateSubscription(s)) { ReconnectWaitMs = 50 };

            sutSubscriber = sut;
        }

        [Fact]
        public void Should_return_original_subscription_as_sender()
        {
            var sender = sut.Sender;

            Assert.Same(eventSubscription, sender);
        }

        [Fact]
        public void Should_subscribe_after_constructor()
        {
            sut.Unsubscribe();

            A.CallTo(() => eventStore.CreateSubscription(sut, A<string>._, A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_reopen_subscription_once_if_exception_is_retrieved()
        {
            await OnErrorAsync(eventSubscription, new InvalidOperationException());

            await Task.Delay(1000);

            sut.Unsubscribe();

            A.CallTo(() => eventSubscription.Unsubscribe())
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>._, A<string>._, A<string>._))
                .MustHaveHappened(2, Times.Exactly);

            A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, A<Exception>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_error_from_inner_subscription_if_failed_often()
        {
            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);

            sut.Unsubscribe();

            A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_unsubscribe_after_last_error_to_keep_sender()
        {
            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(eventSubscription, ex);

            A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, ex))
                .MustHaveHappened();

            Assert.NotNull(sut.Sender);

            sut.Unsubscribe();
        }

        [Fact]
        public async Task Should_not_forward_error_if_exception_is_raised_after_unsubscribe()
        {
            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);

            sut.Unsubscribe();

            A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, A<Exception>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_event_from_inner_subscription()
        {
            var ev = new StoredEvent("Stream", "1", 2, new EventData("Type", new EnvelopeHeaders(), "Payload"));

            await OnEventAsync(eventSubscription, ev);

            sut.Unsubscribe();

            A.CallTo(() => eventSubscriber.OnEventAsync(eventSubscription, ev))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_event_if_message_is_from_another_subscription()
        {
            var ev = new StoredEvent("Stream", "1", 2, new EventData("Type", new EnvelopeHeaders(), "Payload"));

            await OnEventAsync(A.Fake<IEventSubscription>(), ev);

            sut.Unsubscribe();

            A.CallTo(() => eventSubscriber.OnEventAsync(A<IEventSubscription>._, A<StoredEvent>._))
                .MustHaveHappened();
        }

        private Task OnErrorAsync(IEventSubscription subscriber, Exception ex)
        {
            return sutSubscriber.OnErrorAsync(subscriber, ex);
        }

        private Task OnEventAsync(IEventSubscription subscriber, StoredEvent ev)
        {
            return sutSubscriber.OnEventAsync(subscriber, ev);
        }
    }
}