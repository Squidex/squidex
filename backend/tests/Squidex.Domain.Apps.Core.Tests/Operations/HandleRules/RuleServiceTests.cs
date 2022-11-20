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
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public class RuleServiceTests
{
    private readonly IRuleTriggerHandler ruleTriggerHandler = A.Fake<IRuleTriggerHandler>();
    private readonly IRuleActionHandler ruleActionHandler = A.Fake<IRuleActionHandler>();
    private readonly IEventEnricher eventEnricher = A.Fake<IEventEnricher>();
    private readonly IClock clock = A.Fake<IClock>();
    private readonly string actionData = "{\"value\":10}";
    private readonly string actionDump = "MyDump";
    private readonly string actionName = "ValidAction";
    private readonly string actionDescription = "MyDescription";
    private readonly DomainId ruleId = DomainId.NewGuid();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly TypeRegistry typeRegistry = new TypeRegistry();
    private readonly RuleService sut;

    public sealed class InvalidEvent : IEvent
    {
    }

    public sealed record InvalidAction : RuleAction
    {
    }

    public sealed record ValidAction : RuleAction
    {
    }

    public sealed class ValidData
    {
        public int Value { get; set; }
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
        typeRegistry.Add<RuleAction, ValidAction>(actionName);

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());

        A.CallTo(() => ruleActionHandler.ActionType)
            .Returns(typeof(ValidAction));

        A.CallTo(() => ruleActionHandler.DataType)
            .Returns(typeof(ValidData));

        A.CallTo(() => ruleTriggerHandler.TriggerType)
            .Returns(typeof(ContentChangedTriggerV2));

        var log = A.Fake<ILogger<RuleService>>();

        sut = new RuleService(Options.Create(new RuleOptions()),
            new[] { ruleTriggerHandler },
            new[] { ruleActionHandler },
            eventEnricher, TestUtils.DefaultSerializer, log, typeRegistry)
        {
            Clock = clock
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
        var context = RuleInvalidTrigger();

        var actual = sut.CanCreateSnapshotEvents(context);

        Assert.False(actual);
    }

    [Fact]
    public void Should_not_run_from_snapshots_if_trigger_handler_does_not_support_it()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(false);

        var actual = sut.CanCreateSnapshotEvents(context);

        Assert.False(actual);
    }

    [Fact]
    public void Should_run_from_snapshots_if_trigger_handler_does_support_it()
    {
        var context = Rule();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var actual = sut.CanCreateSnapshotEvents(context);

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
        var context = RuleInvalidTrigger();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Empty(jobs);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_create_job_from_snapshots_if_no_action_handler_registered()
    {
        var context = RuleInvalidAction();

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        var jobs = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

        Assert.Empty(jobs);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_jobs_from_snapshots_if_rule_disabled_but_included()
    {
        var context = Rule(disable: true, includeSkipped: true);

        A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId }
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

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId }
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

        A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context))
            .Throws(new InvalidOperationException());

        A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(context, default))
            .Returns(new List<EnrichedEvent>
            {
                new EnrichedContentEvent { AppId = appId },
                new EnrichedContentEvent { AppId = appId }
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

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.WrongEvent, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_if_no_trigger_handler_registered(bool includeSkipped)
    {
        var context = RuleInvalidTrigger(includeSkipped);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.NoTrigger, job.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_if_trigger_handler_does_not_handle_event(bool includeSkipped)
    {
        var context = Rule(includeSkipped: includeSkipped);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.WrongEventForTrigger, job.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_create_debug_job_if_no_action_handler_registered(bool includeSkipped)
    {
        var context = RuleInvalidAction(includeSkipped);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.NoAction, job.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_debug_job_if_rule_disabled()
    {
        var context = Rule(disable: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.Disabled, actual.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_job_if_rule_disabled_and_skipped_included()
    {
        var context = Rule(disable: true, includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.Disabled);
    }

    [Fact]
    public async Task Should_create_debug_job_if_too_old()
    {
        var context = Rule();

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.TooOld, job.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_create_job_if_too_old_but_stale_events_are_included()
    {
        var context = Rule(includeStale: true);

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.None);
    }

    [Fact]
    public async Task Should_create_job_if_too_old_but_skipped_are_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope =
            Envelope.Create(new ContentCreated())
                .SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.TooOld);
    }

    [Fact]
    public async Task Should_create_debug_job_if_event_created_by_rule()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated { FromRule = true });

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.FromRule, job.SkipReason);

        A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
            .MustNotHaveHappened();

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._, A<RuleContext>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_job_if_event_created_by_rule_but_skipped_are_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated { FromRule = true });
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

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

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(false);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.ConditionPrecheckDoesNotMatch, job.SkipReason);
        Assert.Null(job.EnrichedEvent);
        Assert.Null(job.Job);
    }

    [Fact]
    public async Task Should_create_job_if_not_triggered_with_precheck_but_skipped_are_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(false);

        var actual = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, actual, SkipReason.ConditionPrecheckDoesNotMatch);
    }

    [Fact]
    public async Task Should_create_debug_job_if_condition_check_failed()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Throws(new InvalidOperationException());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.Failed, job.SkipReason);
    }

    [Fact]
    public async Task Should_not_create_jobs_if_enriched_event_not_created()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Returns(AsyncEnumerable.Empty<EnrichedEvent>());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).ToListAsync();

        Assert.Empty(job);
    }

    [Fact]
    public async Task Should_create_debug_job_if_not_triggered()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, context))
            .Returns(false);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.ConditionDoesNotMatch, job.SkipReason);
        Assert.Equal(eventEnriched, job.EnrichedEvent);
    }

    [Fact]
    public async Task Should_debug_job_if_not_triggered_but_skipped_included()
    {
        var context = Rule(includeSkipped: true);

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, context))
            .Returns(false);

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, job, SkipReason.ConditionDoesNotMatch);
    }

    [Fact]
    public async Task Should_create_debug_job_if_enrichment_failed()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Throws(new InvalidOperationException());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.Equal(SkipReason.Failed, job.SkipReason);
    }

    [Fact]
    public async Task Should_create_job_if_triggered()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        AssertJob(eventEnriched, job, SkipReason.None);

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched, MatchPayload(eventEnvelope)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_create_job_with_exception_if_trigger_failed()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched = SetupFullFlow(context, eventEnvelope);

        A.CallTo(() => ruleActionHandler.CreateJobAsync(eventEnriched, context.Rule.Action))
            .Throws(new InvalidOperationException());

        var job = await sut.CreateJobsAsync(eventEnvelope, context).SingleAsync();

        Assert.NotNull(job.EnrichmentError);
        Assert.NotNull(job.Job?.ActionData);
        Assert.NotNull(job.Job?.Description);
        Assert.Equal(eventEnriched, job.EnrichedEvent);

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched, MatchPayload(eventEnvelope)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_create_multiple_jobs_if_triggered()
    {
        var context = Rule();

        var eventEnvelope = CreateEnvelope(new ContentCreated());
        var eventEnriched1 = new EnrichedContentEvent { AppId = appId };
        var eventEnriched2 = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched1, context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched2, context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Returns(new List<EnrichedEvent> { eventEnriched1, eventEnriched2 }.ToAsyncEnumerable());

        A.CallTo(() => ruleActionHandler.CreateJobAsync(eventEnriched1, context.Rule.Action))
            .Returns((actionDescription, new ValidData { Value = 10 }));

        A.CallTo(() => ruleActionHandler.CreateJobAsync(eventEnriched2, context.Rule.Action))
            .Returns((actionDescription, new ValidData { Value = 10 }));

        var jobs = await sut.CreateJobsAsync(eventEnvelope, context, default).ToListAsync();

        AssertJob(eventEnriched1, jobs[0], SkipReason.None);
        AssertJob(eventEnriched2, jobs[1], SkipReason.None);

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched1, MatchPayload(eventEnvelope)))
            .MustHaveHappened();

        A.CallTo(() => eventEnricher.EnrichAsync(eventEnriched2, MatchPayload(eventEnvelope)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_succeeded_job_with_full_dump_if_handler_returns_no_exception()
    {
        A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
            .Returns(Result.Success(actionDump));

        var actual = await sut.InvokeAsync(actionName, actionData);

        Assert.Equal(RuleResult.Success, actual.Result.Status);

        Assert.True(actual.Elapsed >= TimeSpan.Zero);
        Assert.True(actual.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_return_failed_job_with_full_dump_if_handler_returns_exception()
    {
        A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
            .Returns(Result.Failed(new InvalidOperationException(), actionDump));

        var actual = await sut.InvokeAsync(actionName, actionData);

        Assert.Equal(RuleResult.Failed, actual.Result.Status);

        Assert.True(actual.Elapsed >= TimeSpan.Zero);
        Assert.True(actual.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_return_timedout_job_with_full_dump_if_exception_from_handler_indicates_timeout()
    {
        A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
            .Returns(Result.Failed(new TimeoutException(), actionDump));

        var actual = await sut.InvokeAsync(actionName, actionData);

        Assert.Equal(RuleResult.Timeout, actual.Result.Status);

        Assert.True(actual.Elapsed >= TimeSpan.Zero);
        Assert.True(actual.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));

        Assert.True(actual.Result.Dump?.IndexOf("Action timed out.", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    [Fact]
    public async Task Should_create_exception_details_if_job_to_execute_failed()
    {
        var ex = new InvalidOperationException();

        A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
            .Throws(ex);

        var actual = await sut.InvokeAsync(actionName, actionData);

        Assert.Equal(ex, actual.Result.Exception);
    }

    private EnrichedContentEvent SetupFullFlow<T>(RuleContext context, Envelope<T> eventEnvelope) where T : AppEvent
    {
        var eventEnriched = new EnrichedContentEvent { AppId = appId };

        A.CallTo(() => ruleTriggerHandler.Handles(eventEnvelope.Payload))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(eventEnvelope), context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.Trigger(eventEnriched, context))
            .Returns(true);

        A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(eventEnvelope), context, default))
            .Returns(new List<EnrichedEvent> { eventEnriched }.ToAsyncEnumerable());

        A.CallTo(() => ruleActionHandler.CreateJobAsync(eventEnriched, context.Rule.Action))
            .Returns((actionDescription, new ValidData { Value = 10 }));

        return eventEnriched;
    }

    private RuleContext RuleInvalidAction(bool includeSkipped = false)
    {
        return new RuleContext
        {
            AppId = appId,
            Rule = new Rule(new ContentChangedTriggerV2(), new InvalidAction()),
            RuleId = ruleId,
            IncludeSkipped = includeSkipped
        };
    }

    private RuleContext RuleInvalidTrigger(bool includeSkipped = false)
    {
        return new RuleContext
        {
            AppId = appId,
            Rule = new Rule(new InvalidTrigger(), new ValidAction()),
            RuleId = ruleId,
            IncludeSkipped = includeSkipped
        };
    }

    private RuleContext Rule(bool disable = false, bool includeStale = false, bool includeSkipped = false)
    {
        var rule = new Rule(new ContentChangedTriggerV2(), new ValidAction());

        if (disable)
        {
            rule = rule.Disable();
        }

        return new RuleContext
        {
            AppId = appId,
            Rule = rule,
            RuleId = ruleId,
            IncludeStale = includeStale,
            IncludeSkipped = includeSkipped
        };
    }

    private Envelope<T> CreateEnvelope<T>(T @event) where T : class, IEvent
    {
        return Envelope.Create(@event).SetTimestamp(clock.GetCurrentInstant());
    }

    private static Envelope<AppEvent> MatchPayload(Envelope<IEvent> eventEnvelope)
    {
        return A<Envelope<AppEvent>>.That.Matches(x => x.Payload == eventEnvelope.Payload);
    }

    private void AssertJob(EnrichedContentEvent eventEnriched, JobResult actual, SkipReason skipped)
    {
        var now = clock.GetCurrentInstant();

        var job = actual.Job!;

        Assert.Equal(skipped, actual.SkipReason);

        Assert.Equal(eventEnriched, actual.EnrichedEvent);
        Assert.Equal(eventEnriched.AppId.Id, job.AppId);

        Assert.Equal(actionData, job.ActionData);
        Assert.Equal(actionName, job.ActionName);
        Assert.Equal(actionDescription, job.Description);

        Assert.Equal(now, job.Created);
        Assert.Equal(now.Plus(Duration.FromDays(30)), job.Expires);

        Assert.NotEqual(DomainId.Empty, job.Id);
    }
}
