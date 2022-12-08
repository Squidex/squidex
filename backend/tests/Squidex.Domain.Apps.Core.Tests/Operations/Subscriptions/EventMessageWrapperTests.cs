// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class EventMessageWrapperTests
{
    private readonly ISubscriptionEventCreator creator1 = A.Fake<ISubscriptionEventCreator>();
    private readonly ISubscriptionEventCreator creator2 = A.Fake<ISubscriptionEventCreator>();

    [Fact]
    public async Task Should_return_event_from_first_creator()
    {
        var enrichedEvent = new EnrichedContentEvent();

        var envelope = Envelope.Create<AppEvent>(new AppCreated());

        A.CallTo(() => creator1.Handles(envelope.Payload))
            .Returns(true);

        A.CallTo(() => creator1.CreateEnrichedEventsAsync(envelope, default))
            .Returns(null!);

        A.CallTo(() => creator2.Handles(envelope.Payload))
            .Returns(true);

        A.CallTo(() => creator2.CreateEnrichedEventsAsync(envelope, default))
            .Returns(enrichedEvent);

        var sut = new EventMessageWrapper(envelope, new[] { creator1, creator2 });

        var actual = await sut.CreatePayloadAsync();

        Assert.Same(enrichedEvent, actual);
    }

    [Fact]
    public async Task Should_not_invoke_creator_if_it_does_not_handle_event()
    {
        var enrichedEvent = new EnrichedContentEvent();

        var envelope = Envelope.Create<AppEvent>(new AppCreated());

        A.CallTo(() => creator1.Handles(envelope.Payload))
            .Returns(false);

        var sut = new EventMessageWrapper(envelope, new[] { creator1 });

        Assert.Null(await sut.CreatePayloadAsync());

        A.CallTo(() => creator1.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
