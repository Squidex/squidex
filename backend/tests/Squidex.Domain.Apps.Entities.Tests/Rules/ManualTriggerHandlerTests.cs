// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class ManualTriggerHandlerTests
{
    private readonly IRuleTriggerHandler sut = new ManualTriggerHandler();

    [Fact]
    public void Should_return_false_if_asking_for_snapshot_support()
    {
        Assert.False(sut.CanCreateSnapshotEvents);
    }

    [Fact]
    public void Should_calculate_name()
    {
        var @event = new RuleManuallyTriggered();

        Assert.Equal("Manual", sut.GetName(@event));
    }

    [Fact]
    public async Task Should_create_event_with_name()
    {
        var @event = TestUtils.CreateEvent<RuleManuallyTriggered>();
        var envelope = Envelope.Create<AppEvent>(@event);

        var actual = await sut.CreateEnrichedEventsAsync(envelope, default, default).ToListAsync();

        var enrichedEvent = (EnrichedManualEvent)actual.Single();

        Assert.Equal(@event.Actor, enrichedEvent.Actor);
        Assert.Equal(@event.AppId, enrichedEvent.AppId);
        Assert.Equal(@event.AppId.Id, enrichedEvent.AppId.Id);
    }

    [Fact]
    public async Task Should_create_event_with_actor()
    {
        var actor = RefToken.User("me");

        var @event = new RuleManuallyTriggered { Actor = actor };
        var envelope = Envelope.Create<AppEvent>(@event);

        var actual = await sut.CreateEnrichedEventsAsync(envelope, default, default).ToListAsync();

        Assert.Equal(actor, ((EnrichedUserEventBase)actual.Single()).Actor);
    }

    [Fact]
    public void Should_always_trigger()
    {
        var @event = new RuleManuallyTriggered();

        Assert.True(sut.Trigger(Envelope.Create<AppEvent>(@event), default));
    }

    [Fact]
    public void Should_always_trigger_enriched_event()
    {
        var @event = new EnrichedUsageExceededEvent();

        Assert.True(sut.Trigger(@event, default));
    }
}
