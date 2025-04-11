// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Flows.Internal;
using Squidex.Flows.Steps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public class RuleServiceTests
{
    private readonly IRuleTriggerHandler ruleTriggerHandler = A.Fake<IRuleTriggerHandler>();
    private readonly IEventEnricher eventEnricher = A.Fake<IEventEnricher>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant().WithoutMs();
    private readonly RuleService sut;

    public sealed class InvalidEvent : IEvent
    {
    }

    public sealed record InvalidTrigger : RuleTrigger
    {
        public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
        {
            return default!;
        }
    }

    public RuleServiceTests()
    {
        var clock = A.Fake<IClock>();

        A.CallTo(() => clock.GetCurrentInstant())
            .ReturnsLazily(() => now);

        A.CallTo(() => ruleTriggerHandler.TriggerType)
            .Returns(typeof(ContentChangedTriggerV2));

        sut = new RuleService(
            [ruleTriggerHandler],
            eventEnricher,
            Options.Create(new RulesOptions()),
            A.Fake<ILogger<RuleService>>())
        {
            Clock = clock,
        };
    }

    [Fact]
    public void Should_calculate_event_name_from_trigger_handler()
    {
        var eventEnvelope = new ContentCreated();

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.GetName(eventEnvelope))
            .Returns("custom-name");

        var name = sut.GetName(eventEnvelope);

        Assert.Equal("custom-name", name);
    }

    [Fact]
    public void Should_calculate_default_name_if_trigger_handler_returns_no_name()
    {
        var eventEnvelope = new ContentCreated();

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.GetName(eventEnvelope))
            .Returns(null);

        var name = sut.GetName(eventEnvelope);

        Assert.Equal("ContentCreated", name);

        A.CallTo(() => ruleTriggerHandler.GetName(eventEnvelope))
            .MustHaveHappened();
    }

    [Fact]
    public void Should_calculate_default_name_if_trigger_handler_cannot_not_handle_event()
    {
        var eventEnvelope = new ContentCreated();

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope))
            .Returns(false);

        var name = sut.GetName(eventEnvelope);

        Assert.Equal("ContentCreated", name);

        A.CallTo(() => ruleTriggerHandler.GetName(eventEnvelope))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Should_not_run_from_snapshots_if_no_trigger_handler_registered()
    {
        var context = Rule(trigger: new InvalidTrigger());

        var actual = sut.CanCreateSnapshotEvents(context.Rule);

        Assert.False(actual);
    }

    [Fact]
    public void Should_not_run_from_snapshots_if_trigger_handler_does_not_support_it()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(false);

        var actual = sut.CanCreateSnapshotEvents(context.Rule);

        Assert.False(actual);
    }

    [Fact]
    public void Should_run_from_snapshots_if_trigger_handler_does_support_it()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var actual = sut.CanCreateSnapshotEvents(context.Rule);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_create_job_from_snapshots_if_trigger_handler_does_not_support_it()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(false);

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Empty(jobs);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_job_from_snapshots_if_rule_disabled()
    {
        var context = Rule(disable: true);

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Empty(jobs);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_job_from_snapshots_if_no_trigger_handler_registered()
    {
        var context = Rule(trigger: new InvalidTrigger());

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Empty(jobs);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_jobs_from_snapshots_if_rule_disabled_and_included()
    {
        var context = Rule(disable: true, includeSkipped: true);

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context.Rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId },
            }.ToAsyncEnumerable());

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Equal(2, jobs.Count(x => x.Job != null && x.EnrichmentError == null));
    }

    [Fact]
    public async Task Should_create_jobs_from_snapshots()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context.Rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId },
            }.ToAsyncEnumerable());

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Equal(2, jobs.Count(x => x.Job != null && x.EnrichmentError == null));
    }

    [Fact]
    public async Task Should_create_jobs_with_exceptions_from_snapshots()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context.Rule.Trigger))
            .Throws(new InvalidOperationException());

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId },
            }.ToAsyncEnumerable());

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Equal(2, jobs.Count(x => x.Job == null && x.EnrichmentError != null));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_for_invalid_event(bool includeSkipped)
    {
        var context = Rule(includeSkipped: includeSkipped);

        var eventEnvelope = CreateEnvelope(new InvalidEvent());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.WrongEvent, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleTrigger>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_if_no_trigger_handler_registered(bool includeSkipped)
    {
        var context = Rule(includeSkipped: includeSkipped, trigger: new InvalidTrigger());

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.NoTrigger, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleTrigger>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_if_trigger_handler_does_not_handle_event(bool includeSkipped)
    {
        var context = Rule(includeSkipped: includeSkipped);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.WrongEventForTrigger, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleTrigger>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_debug_job_if_rule_disabled()
    {
        var context = Rule(disable: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.Disabled, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleTrigger>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_job_if_rule_disabled_and_included()
    {
        var context = Rule(disable: true, includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.Disabled);
    }

    [Fact]
    public async Task Should_create_debug_job_if_too_old()
    {
        var context = Rule();

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(now.Minus(Duration.FromDays(3)));

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.TooOld, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleTrigger>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_job_if_too_old_but_stale_events_are_included()
    {
        var context = Rule(includeStale: true);

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(now.Minus(Duration.FromDays(3)));
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.None);
    }

    [Fact]
    public async Task Should_create_job_if_too_old_and_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(now.Minus(Duration.FromDays(3)));
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.TooOld);
    }

    [Fact]
    public async Task Should_create_debug_job_if_event_created_by_rule()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated { FromRule = true });

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.FromRule, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, context.Rule.Trigger))
            .MustNotHaveHappened();

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._, A<RulesContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_debug_job_if_event_created_by_rule_and_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated { FromRule = true });
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.FromRule);
    }

    [Fact]
    public async Task Should_create_debug_job_if_not_triggered_with_precheck()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Returns(false);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context.ToRulesContext(), default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.ConditionPrecheckDoesNotMatch, actual.SkipReason);
        Assert.Null(actual.EnrichedEvent);
        Assert.Null(actual.Job);
    }

    [Fact]
    public async Task Should_create_job_if_not_triggered_with_precheck_and_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Returns(false);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.ConditionPrecheckDoesNotMatch);
    }

    [Fact]
    public async Task Should_create_debug_job_if_condition_check_failed()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Throws(new InvalidOperationException());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.Failed, actual.SkipReason);
    }

    [Fact]
    public async Task Should_not_create_jobs_if_enriched_event_not_created()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context.ToRulesContext(), default))
            .Returns(AsyncEnumerable.Empty<EnrichedEvent>());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).ToListAsync();

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_create_skipped_result_if_not_triggered_and_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, context.Rule.Trigger))
            .Returns(false);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context.ToRulesContext(), default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.ConditionDoesNotMatch, actual.SkipReason);
        Assert.Equal(eventEnriched, actual.EnrichedEvent);
    }

    [Fact]
    public async Task Should_create_debug_job_if_not_triggered_and_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, context.Rule.Trigger))
            .Returns(false);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.ConditionDoesNotMatch);
    }

    [Fact]
    public async Task Should_skipped_result_if_enrichment_failed()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context.Rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context.ToRulesContext(), default))
            .Throws(new InvalidOperationException());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        Assert.Equal(SkipReason.Failed, actual.SkipReason);
    }

    [Fact]
    public async Task Should_create_job_if_triggered()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = CreateDefaultFlow(context.Rule, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.None);

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched, MatchPayload(eventEnvelope)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_create_multiple_jobs_if_triggered()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched1 = CreateDefaultFlow(context.Rule, eventEnvelope);
        var eventEnriched2 = CreateDefaultFlow(context.Rule, eventEnvelope);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), A<RulesContext>._, default))
            .Returns(new List<EnrichedEvent> { eventEnriched1, eventEnriched2 }.ToAsyncEnumerable());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context.ToRulesContext()).ToListAsync();

        AssertJob(eventEnriched1, actual[0], SkipReason.None);
        AssertJob(eventEnriched2, actual[1], SkipReason.None);

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched1, MatchPayload(eventEnvelope)))
            .MustHaveHappened();

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched2, MatchPayload(eventEnvelope)))
            .MustHaveHappened();
    }

    private EnrichedContentEvent CreateDefaultFlow<T>(Rule rule, Envelope<T> eventEnvelope) where T : AppEvent
    {
        var eventEnriched = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, rule.Trigger))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), A<RulesContext>._, default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        return eventEnriched;
    }

    private RuleContext Rule(bool disable = false, bool includeStale = false, bool includeSkipped = false, RuleTrigger? trigger = null)
    {
        var stepId = Guid.NewGuid();

        var rule = new Rule
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [stepId] = new FlowStepDefinition { Step = new DelayFlowStep() },
                },
                InitialStep = stepId,
            },
            Trigger = trigger ?? new ContentChangedTriggerV2(),
        };

        if (disable)
        {
            rule = rule.Disable();
        }

        return new RuleContext
        {
            AppId = appId,
            IncludeStale = includeStale,
            IncludeSkipped = includeSkipped,
            Rule = rule,
        };
    }

    private Envelope<T> CreateEnvelope<T>(T @event) where T : class, IEvent
    {
        return Envelope.Create(@event).SetTimestamp(now);
    }

    private static Envelope<AppEvent> MatchPayload(Envelope<IEvent> eventEnvelope)
    {
        return A<Envelope<AppEvent>>.That.Matches(x => x.Payload == eventEnvelope.Payload);
    }

    private static void AssertJob(EnrichedContentEvent eventEnriched, JobResult actual, SkipReason skipped)
    {
        Assert.Equal(skipped, actual.SkipReason);
        Assert.Equal(eventEnriched, actual.EnrichedEvent);
    }
}
