// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking;

public class UsageTriggerHandlerTests : GivenContext
{
    private readonly IRuleTriggerHandler sut = new UsageTriggerHandler();

    [Fact]
    public void Should_return_false_if_asking_for_snapshot_support()
    {
        Assert.False(sut.CanCreateSnapshotEvents);
    }

    [Fact]
    public void Should_handle_usage_event()
    {
        Assert.True(sut.Handles(new AppUsageExceeded()));
    }

    [Fact]
    public void Should_not_handle_other_event()
    {
        Assert.False(sut.Handles(new ContentCreated()));
    }

    [Fact]
    public async Task Should_create_enriched_event()
    {
        var ctx = Context().ToRulesContext();

        var @event = new AppUsageExceeded { CallsCurrent = 80, CallsLimit = 120 };
        var envelope = Envelope.Create<AppEvent>(@event);

        var actuals = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();
        var actual = actuals.Single() as EnrichedUsageExceededEvent;

        Assert.Equal(@event.CallsCurrent, actual!.CallsCurrent);
        Assert.Equal(@event.CallsLimit, actual!.CallsLimit);
    }

    [Fact]
    public void Should_not_trigger_precheck_if_rule_id_not_matchs()
    {
        var ctx = Context();

        var @event = new AppUsageExceeded();

        var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

        Assert.True(actual);
    }

    [Fact]
    public void Should_trigger_precheck_if_event_type_correct_and_rule_id_matchs()
    {
        var ctx = Context();

        var @event = new AppUsageExceeded { RuleId = ctx.Rule.Id };

        var actual = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx.Rule.Trigger);

        Assert.True(actual);
    }

    private RuleContext Context(RuleTrigger? trigger = null)
    {
        trigger ??= new UsageTrigger();

        return new RuleContext
        {
            AppId = AppId,
            IncludeSkipped = false,
            IncludeStale = false,
            Rule = CreateRule() with { Trigger = trigger }
        };
    }
}
