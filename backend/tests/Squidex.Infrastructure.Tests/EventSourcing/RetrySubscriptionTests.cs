// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public class RetrySubscriptionTests
{
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventSubscriber<StoredEvent> eventSubscriber = A.Fake<IEventSubscriber<StoredEvent>>();
    private readonly IEventSubscription eventSubscription = A.Fake<IEventSubscription>();
    private readonly IEventSubscriber<StoredEvent> sutSubscriber;
    private readonly RetrySubscription<StoredEvent> sut;

    public RetrySubscriptionTests()
    {
        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .Returns(eventSubscription);

        sut = new RetrySubscription<StoredEvent>(eventSubscriber, s => eventStore.CreateSubscription(s)) { ReconnectWaitMs = 50 };
        sutSubscriber = sut;
    }

    [Fact]
    public void Should_subscribe_after_constructor()
    {
        sut.Dispose();

        A.CallTo(() => eventStore.CreateSubscription(sut, A<string>._, A<string>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_reopen_subscription_once_if_exception_is_retrieved()
    {
        var ex = new InvalidOperationException();

        await OnErrorAsync(eventSubscription, ex, times: 1);

        await Task.Delay(1000);

        sut.Dispose();

        A.CallTo(() => eventSubscription.Dispose())
            .MustHaveHappened(2, Times.Exactly);

        A.CallTo(() => eventStore.CreateSubscription(A<IEventSubscriber<StoredEvent>>._, A<string>._, A<string>._))
            .MustHaveHappened(2, Times.Exactly);

        A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, A<Exception>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_forward_error_from_inner_subscription_if_failed_often()
    {
        var ex = new InvalidOperationException();

        await OnErrorAsync(eventSubscription, ex, times: 6);

        sut.Dispose();

        A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_operation_cancelled_error_from_inner_subscription_if_failed_often()
    {
        var ex = new OperationCanceledException();

        await OnErrorAsync(eventSubscription, ex, times: 6);

        sut.Dispose();

        A.CallTo(() => eventSubscriber.OnErrorAsync(sut, ex))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_forward_error_if_exception_is_raised_after_unsubscribe()
    {
        var ex = new InvalidOperationException();

        await OnErrorAsync(eventSubscription, ex, times: 1);

        sut.Dispose();

        A.CallTo(() => eventSubscriber.OnErrorAsync(eventSubscription, A<Exception>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_forward_event_from_inner_subscription()
    {
        var @event = new StoredEvent("Stream", "1", 2, new EventData("Type", new EnvelopeHeaders(), "Payload"));

        await OnNextAsync(eventSubscription, @event);

        sut.Dispose();

        A.CallTo(() => eventSubscriber.OnNextAsync(sut, @event))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_forward_event_if_message_is_from_another_subscription()
    {
        var @event = new StoredEvent("Stream", "1", 2, new EventData("Type", new EnvelopeHeaders(), "Payload"));

        await OnNextAsync(A.Fake<IEventSubscription>(), @event);

        sut.Dispose();

        A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_be_able_to_unsubscribe_within_exception_handler()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => eventSubscriber.OnErrorAsync(A<IEventSubscription>._, A<Exception>._))
            .Invokes(() => sut.Dispose());

        await OnErrorAsync(eventSubscription, ex, times: 6);

        Assert.False(sut.IsSubscribed);
    }

    [Fact]
    public async Task Should_be_able_to_unsubscribe_within_event_handler()
    {
        var @event = new StoredEvent("Stream", "1", 2, new EventData("Type", new EnvelopeHeaders(), "Payload"));

        A.CallTo(() => eventSubscriber.OnNextAsync(A<IEventSubscription>._, A<StoredEvent>._))
            .Invokes(() => sut.Dispose());

        await OnNextAsync(eventSubscription, @event);

        Assert.False(sut.IsSubscribed);
    }

    private async ValueTask OnErrorAsync(IEventSubscription subscriber, Exception ex, int times)
    {
        for (var i = 0; i < times; i++)
        {
            await sutSubscriber.OnErrorAsync(subscriber, ex);
        }
    }

    private ValueTask OnNextAsync(IEventSubscription subscriber, StoredEvent ev)
    {
        return sutSubscriber.OnNextAsync(subscriber, ev);
    }
}
