// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
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
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
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
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
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
            typeNameRegistry.Map(typeof(ContentCreated));
            typeNameRegistry.Map(typeof(ValidAction), actionName);

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());

            A.CallTo(() => ruleActionHandler.ActionType)
                .Returns(typeof(ValidAction));

            A.CallTo(() => ruleActionHandler.DataType)
                .Returns(typeof(ValidData));

            A.CallTo(() => ruleTriggerHandler.TriggerType)
                .Returns(typeof(ContentChangedTriggerV2));

            var log = A.Fake<ISemanticLog>();

            sut = new RuleService(Options.Create(new RuleOptions()),
                new[] { ruleTriggerHandler },
                new[] { ruleActionHandler },
                eventEnricher, TestUtils.DefaultSerializer, clock, log, typeNameRegistry);
        }

        [Fact]
        public void Should_calculate_event_name_from_trigger_handler()
        {
            var @event = new ContentCreated();

            A.CallTo(() => ruleTriggerHandler.Handles(@event))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.GetName(@event))
                .Returns("custom-name");

            var name = sut.GetName(@event);

            Assert.Equal("custom-name", name);
        }

        [Fact]
        public void Should_calculate_default_name_if_trigger_handler_returns_no_name()
        {
            var @event = new ContentCreated();

            A.CallTo(() => ruleTriggerHandler.Handles(@event))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.GetName(@event))
                .Returns(null);

            var name = sut.GetName(@event);

            Assert.Equal("ContentCreated", name);

            A.CallTo(() => ruleTriggerHandler.GetName(@event))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_calculate_default_name_if_trigger_handler_cannot_not_handle_event()
        {
            var @event = new ContentCreated();

            A.CallTo(() => ruleTriggerHandler.Handles(@event))
                .Returns(false);

            var name = sut.GetName(@event);

            Assert.Equal("ContentCreated", name);

            A.CallTo(() => ruleTriggerHandler.GetName(@event))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_run_from_snapshots_if_no_trigger_handler_registered()
        {
            var result = sut.CanCreateSnapshotEvents(RuleInvalidTrigger());

            Assert.False(result);
        }

        [Fact]
        public void Should_not_run_from_snapshots_if_trigger_handler_does_not_support_it()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(false);

            var result = sut.CanCreateSnapshotEvents(Rule());

            Assert.False(result);
        }

        [Fact]
        public void Should_run_from_snapshots_if_trigger_handler_does_support_it()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(true);

            var result = sut.CanCreateSnapshotEvents(Rule());

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_create_job_from_snapshots_if_trigger_handler_does_not_support_it()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(false);

            var jobs = await sut.CreateSnapshotJobsAsync(Rule()).ToListAsync();

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_from_snapshots_if_rule_disabled()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(true);

            var jobs = await sut.CreateSnapshotJobsAsync(Rule(disable: true)).ToListAsync();

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_from_snapshots_if_no_trigger_handler_registered()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(true);

            var jobs = await sut.CreateSnapshotJobsAsync(RuleInvalidTrigger()).ToListAsync();

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_from_snapshots_if_no_action_handler_registered()
        {
            A.CallTo(() => ruleTriggerHandler.CanCreateSnapshotEvents)
                .Returns(true);

            var jobs = await sut.CreateSnapshotJobsAsync(RuleInvalidAction()).ToListAsync();

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.CreateSnapshotEventsAsync(A<RuleContext>._, A<CancellationToken>._))
                .MustNotHaveHappened();
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

            var result = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

            Assert.Equal(2, result.Count(x => x.Job != null && x.Exception == null));
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

            var result = await sut.CreateSnapshotJobsAsync(context).ToListAsync();

            Assert.Equal(2, result.Count(x => x.Job == null && x.Exception != null));
        }

        [Fact]
        public async Task Should_create_debug_rob_if_rule_disabled()
        {
            var @event = Envelope.Create(new ContentCreated());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, Rule(disable: true)).SingleAsync();

            Assert.Equal(SkipReason.Disabled, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_for_invalid_event()
        {
            var @event = Envelope.Create(new InvalidEvent());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, Rule()).SingleAsync();

            Assert.Equal(SkipReason.EventMismatch, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_no_trigger_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, RuleInvalidTrigger()).SingleAsync();

            Assert.Equal(SkipReason.NoTrigger, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_trigger_handler_does_not_handle_event()
        {
            var @event = Envelope.Create(new ContentCreated());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, Rule()).SingleAsync();

            Assert.Equal(SkipReason.WrongEventForTrigger, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_no_action_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            var (_, _, reason) = await sut.CreateJobsAsync(@event, RuleInvalidAction()).SingleAsync();

            Assert.Equal(SkipReason.NoAction, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_too_old()
        {
            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            var (_, _, reason) = await sut.CreateJobsAsync(@event, Rule(ignoreStale: true)).SingleAsync();

            Assert.Equal(SkipReason.TooOld, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_job_if_too_old_but_stale_events_are_not_ignored()
        {
            var context = Rule(ignoreStale: false);

            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>._, context))
                .Returns(true);

            var jobs = await sut.CreateJobsAsync(@event, context).ToListAsync();

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_event_created_by_rule()
        {
            var context = Rule();

            var @event = Envelope.Create(new ContentCreated { FromRule = true });

            var (_, _, reason) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.Equal(SkipReason.FromRule, reason);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<Envelope<AppEvent>>._, A<RuleContext>._))
                .MustNotHaveHappened();

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._, A<RuleContext>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_not_triggered_with_precheck()
        {
            var context = Rule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(false);

            var (_, _, reason) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.Equal(SkipReason.ConditionDoesNotMatch, reason);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._, A<RuleContext>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_debug_job_if_condition_check_failed()
        {
            var context = Rule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Throws(new InvalidOperationException());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.Equal(SkipReason.Failed, reason);
        }

        [Fact]
        public async Task Should_not_create_jobs_if_enriched_event_not_created()
        {
            var context = Rule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Returns(AsyncEnumerable.Empty<EnrichedEvent>());

            var jobs = await sut.CreateJobsAsync(@event, context).ToListAsync();

            Assert.Empty(jobs);
        }

        [Fact]
        public async Task Should_create_debug_job_if_not_triggered()
        {
            var context = Rule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, context))
                .Returns(false);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Returns(new List<EnrichedEvent> { enrichedEvent }.ToAsyncEnumerable());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.Equal(SkipReason.ConditionDoesNotMatch, reason);
        }

        [Fact]
        public async Task Should_create_debug_job_if_enrichment_failed()
        {
            var now = clock.GetCurrentInstant();

            var context = Rule();

            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Throws(new InvalidOperationException());

            var (_, _, reason) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.Equal(SkipReason.Failed, reason);
        }

        [Fact]
        public async Task Should_create_job_if_triggered()
        {
            var now = clock.GetCurrentInstant();

            var context = Rule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Returns(new List<EnrichedEvent> { enrichedEvent }.ToAsyncEnumerable());

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent, context.Rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var (job, _, _) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            AssertJob(now, enrichedEvent, job!);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent, MatchPayload(@event)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_create_job_with_exception_if_trigger_failed()
        {
            var now = clock.GetCurrentInstant();

            var context = Rule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Returns(new List<EnrichedEvent> { enrichedEvent }.ToAsyncEnumerable());

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent, context.Rule.Action))
                .Throws(new InvalidOperationException());

            var (job, ex, _) = await sut.CreateJobsAsync(@event, context).SingleAsync();

            Assert.NotNull(ex);
            Assert.NotNull(job?.ActionData);
            Assert.NotNull(job?.Description);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent, MatchPayload(@event)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_create_multiple_jobs_if_triggered()
        {
            var now = clock.GetCurrentInstant();

            var context = Rule();

            var enrichedEvent1 = new EnrichedContentEvent { AppId = appId };
            var enrichedEvent2 = new EnrichedContentEvent { AppId = appId };

            var @event =
                Envelope.Create(new ContentCreated())
                    .SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Handles(@event.Payload))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(MatchPayload(@event), context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent1, context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent2, context))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(MatchPayload(@event), context, default))
                .Returns(new List<EnrichedEvent> { enrichedEvent1, enrichedEvent2 }.ToAsyncEnumerable());

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent1, context.Rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent2, context.Rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var jobs = await sut.CreateJobsAsync(@event, context, default).ToListAsync();

            AssertJob(now, enrichedEvent1, jobs[0].Job!);
            AssertJob(now, enrichedEvent1, jobs[1].Job!);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent1, MatchPayload(@event)))
                .MustHaveHappened();

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent2, MatchPayload(@event)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_succeeded_job_with_full_dump_if_handler_returns_no_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Returns(Result.Success(actionDump));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Success, result.Result.Status);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_failed_job_with_full_dump_if_handler_returns_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Returns(Result.Failed(new InvalidOperationException(), actionDump));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Failed, result.Result.Status);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_timedout_job_with_full_dump_if_exception_from_handler_indicates_timeout()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Returns(Result.Failed(new TimeoutException(), actionDump));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Timeout, result.Result.Status);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));

            Assert.True(result.Result.Dump?.IndexOf("Action timed out.", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task Should_create_exception_details_if_job_to_execute_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Throws(ex);

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(ex, result.Result.Exception);
        }

        private RuleContext RuleInvalidAction()
        {
            return new RuleContext
            {
                AppId = appId,
                Rule = new Rule(new ContentChangedTriggerV2(), new InvalidAction()),
                RuleId = ruleId
            };
        }

        private RuleContext RuleInvalidTrigger()
        {
            return new RuleContext
            {
                AppId = appId,
                Rule = new Rule(new InvalidTrigger(), new ValidAction()),
                RuleId = ruleId
            };
        }

        private RuleContext Rule(bool disable = false, bool ignoreStale = true)
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
                IgnoreStale = ignoreStale
            };
        }

        private static Envelope<AppEvent> MatchPayload(Envelope<IEvent> @event)
        {
            return A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload);
        }

        private void AssertJob(Instant now, EnrichedContentEvent enrichedEvent, RuleJob job)
        {
            Assert.Equal(enrichedEvent.AppId.Id, job.AppId);

            Assert.Equal(actionData, job.ActionData);
            Assert.Equal(actionName, job.ActionName);
            Assert.Equal(actionDescription, job.Description);

            Assert.Equal(now, job.Created);
            Assert.Equal(now.Plus(Duration.FromDays(30)), job.Expires);

            Assert.NotEqual(DomainId.Empty, job.Id);
        }
    }
}
