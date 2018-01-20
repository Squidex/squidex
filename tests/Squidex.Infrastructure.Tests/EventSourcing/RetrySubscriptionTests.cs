// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
        private readonly string streamFilter = Guid.NewGuid().ToString();

        public RetrySubscriptionTests()
        {
            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(eventSubscription);

            sut = new RetrySubscription(eventStore, eventSubscriber, streamFilter, null) { ReconnectWaitMs = 100 };

            sutSubscriber = sut;
        }

        [Fact]
        public async Task Should_subscribe_after_constructor()
        {
            await sut.StopAsync();

            A.CallTo(() => eventStore.CreateSubscription(sut, streamFilter, null))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_reopen_subscription_once_when_exception_is_retrieved()
        {
            await OnErrorAsync(eventSubscription, new InvalidOperationException());

            await Task.Delay(200);

            await sut.StopAsync();

            A.CallTo(() => eventSubscription.StopAsync())
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Twice);

            A.CallTo(() => eventSubscriber.OnErrorAsync(A<IEventSubscription>.Ignored, A<Exception>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_error_from_inner_subscription_when_failed_often()
        {
            var ex = new InvalidOperationException();

            await OnErrorAsync(eventSubscription, ex);
            await OnErrorAsync(null, ex);
            await OnErrorAsync(null, ex);
            await OnErrorAsync(null, ex);
            await OnErrorAsync(null, ex);
            await OnErrorAsync(null, ex);
            await sut.StopAsync();

            A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_forward_error_when_exception_is_from_another_subscription()
        {
            var ex = new InvalidOperationException();

            await OnErrorAsync(A.Fake<IEventSubscription>(), ex);
            await sut.StopAsync();

            A.CallTo(() => eventSubscriber.OnErrorAsync(A<IEventSubscription>.Ignored, A<Exception>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_event_from_inner_subscription()
        {
            var ev = new StoredEvent("1", 2, new EventData());

            await OnEventAsync(eventSubscription, ev);
            await sut.StopAsync();

            A.CallTo(() => eventSubscriber.OnEventAsync(sut, ev))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_forward_event_when_message_is_from_another_subscription()
        {
            var ev = new StoredEvent("1", 2, new EventData());

            await OnEventAsync(A.Fake<IEventSubscription>(), ev);
            await sut.StopAsync();

            A.CallTo(() => eventSubscriber.OnEventAsync(A<IEventSubscription>.Ignored, A<StoredEvent>.Ignored))
                .MustNotHaveHappened();
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