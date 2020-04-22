﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Xunit;

#pragma warning disable xUnit2009 // Do not use boolean check to check for string equality

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
        private readonly Guid ruleId = Guid.NewGuid();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly RuleService sut;

        public sealed class InvalidEvent : IEvent
        {
        }

        public sealed class InvalidAction : RuleAction
        {
        }

        public sealed class ValidAction : RuleAction
        {
        }

        public sealed class ValidData
        {
            public int Value { get; set; }
        }

        public sealed class InvalidTrigger : RuleTrigger
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
        public async Task Should_not_create_job_if_rule_disabled()
        {
            var @event = Envelope.Create(new ContentCreated());

            var jobs = await sut.CreateJobsAsync(ValidRule().Disable(), ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>._, A<RuleTrigger>._, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_for_invalid_event()
        {
            var @event = Envelope.Create(new InvalidEvent());

            var jobs = await sut.CreateJobsAsync(ValidRule(), ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>._, A<RuleTrigger>._, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_trigger_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            var jobs = await sut.CreateJobsAsync(RuleInvalidTrigger(), ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>._, A<RuleTrigger>._, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_action_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            var jobs = await sut.CreateJobsAsync(RuleInvalidAction(), ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>._, A<RuleTrigger>._, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_too_old()
        {
            var @event = Envelope.Create(new ContentCreated()).SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));

            var jobs = await sut.CreateJobsAsync(ValidRule(), ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>._, A<RuleTrigger>._, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered_with_precheck()
        {
            var rule = ValidRule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(false);

            var jobs = await sut.CreateJobsAsync(rule, ruleId, @event);

            Assert.Empty(jobs);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_enriched_event_not_created()
        {
            var rule = ValidRule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(new List<EnrichedEvent>());

            var jobs = await sut.CreateJobsAsync(rule, ruleId, @event);

            Assert.Empty(jobs);
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered()
        {
            var rule = ValidRule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(new List<EnrichedEvent> { enrichedEvent });

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, rule.Trigger))
                .Returns(false);

            var jobs = await sut.CreateJobsAsync(rule, ruleId, @event);

            Assert.Empty(jobs);
        }

        [Fact]
        public async Task Should_create_job_if_triggered()
        {
            var now = clock.GetCurrentInstant();

            var rule = ValidRule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event = Envelope.Create(new ContentCreated()).SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(new List<EnrichedEvent> { enrichedEvent });

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, rule.Trigger))
                .Returns(true);

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent, rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var jobs = (await sut.CreateJobsAsync(rule, ruleId, @event))!;

            var job = jobs.Single();

            AssertJob(now, enrichedEvent, job);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent, A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_create_multiple_jobs_if_triggered()
        {
            var now = clock.GetCurrentInstant();

            var rule = ValidRule();

            var enrichedEvent1 = new EnrichedContentEvent { AppId = appId };
            var enrichedEvent2 = new EnrichedContentEvent { AppId = appId };

            var @event = Envelope.Create(new ContentCreated()).SetTimestamp(now);

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventsAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(new List<EnrichedEvent> { enrichedEvent1, enrichedEvent2 });

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent1, rule.Trigger))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent2, rule.Trigger))
                .Returns(true);

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent1, rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            A.CallTo(() => ruleActionHandler.CreateJobAsync(enrichedEvent2, rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var jobs = (await sut.CreateJobsAsync(rule, ruleId, @event))!;

            AssertJob(now, enrichedEvent1, jobs[0]);
            AssertJob(now, enrichedEvent1, jobs[1]);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent1, A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .MustHaveHappened();

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent2, A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_succeeded_job_with_full_dump_when_handler_returns_no_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Returns(Result.Success(actionDump));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Success, result.Result.Status);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_failed_job_with_full_dump_when_handler_returns_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Returns(Result.Failed(new InvalidOperationException(), actionDump));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Failed, result.Result.Status);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Result.Dump?.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_timedout_job_with_full_dump_when_exception_from_handler_indicates_timeout()
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
        public async Task Should_create_exception_details_when_job_to_execute_failed()
        {
            var ex = new InvalidOperationException();

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10), A<CancellationToken>._))
                .Throws(ex);

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(ex, result.Result.Exception);
        }

        private static Rule RuleInvalidAction()
        {
            return new Rule(new ContentChangedTriggerV2(), new InvalidAction());
        }

        private static Rule RuleInvalidTrigger()
        {
            return new Rule(new InvalidTrigger(), new ValidAction());
        }

        private static Rule ValidRule()
        {
            return new Rule(new ContentChangedTriggerV2(), new ValidAction());
        }

        private void AssertJob(Instant now, EnrichedContentEvent enrichedEvent, RuleJob job)
        {
            Assert.Equal(enrichedEvent.AppId.Id, job.AppId);

            Assert.Equal(actionData, job.ActionData);
            Assert.Equal(actionName, job.ActionName);
            Assert.Equal(actionDescription, job.Description);

            Assert.Equal(now, job.Created);
            Assert.Equal(now.Plus(Duration.FromDays(30)), job.Expires);

            Assert.NotEqual(Guid.Empty, job.Id);
        }
    }
}
