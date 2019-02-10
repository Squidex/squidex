// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
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
                return default;
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

            sut = new RuleService(new[] { ruleTriggerHandler }, new[] { ruleActionHandler }, eventEnricher, TestUtils.DefaultSerializer, clock, log, typeNameRegistry);
        }

        [Fact]
        public async Task Should_not_create_job_if_rule_disabled()
        {
            var @event = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(ValidRule().Disable(), ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>.Ignored, A<RuleTrigger>.Ignored, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_for_invalid_event()
        {
            var @event = Envelope.Create(new InvalidEvent());

            var job = await sut.CreateJobAsync(ValidRule(), ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>.Ignored, A<RuleTrigger>.Ignored, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_trigger_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(RuleInvalidTrigger(), ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>.Ignored, A<RuleTrigger>.Ignored, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_action_handler_registered()
        {
            var @event = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(RuleInvalidAction(), ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>.Ignored, A<RuleTrigger>.Ignored, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_too_old()
        {
            var @event = Envelope.Create(new ContentCreated()).SetTimestamp(clock.GetCurrentInstant().Minus(Duration.FromDays(3)));

            var job = await sut.CreateJobAsync(ValidRule(), ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<AppEvent>.Ignored, A<RuleTrigger>.Ignored, ruleId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered_with_precheck()
        {
            var rule = ValidRule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(false);

            var job = await sut.CreateJobAsync(rule, ruleId, @event);

            Assert.Null(job);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_enriched_event_not_created()
        {
            var rule = ValidRule();

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(Task.FromResult<EnrichedEvent>(null));

            var job = await sut.CreateJobAsync(rule, ruleId, @event);

            Assert.Null(job);
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered()
        {
            var rule = ValidRule();

            var enrichedEvent = new EnrichedContentEvent { AppId = appId };

            var @event = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(@event.Payload, rule.Trigger, ruleId))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(enrichedEvent);

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, rule.Trigger))
                .Returns(false);

            var job = await sut.CreateJobAsync(rule, ruleId, @event);

            Assert.Null(job);
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

            A.CallTo(() => ruleTriggerHandler.Trigger(enrichedEvent, rule.Trigger))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.CreateEnrichedEventAsync(A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .Returns(enrichedEvent);

            A.CallTo(() => ruleActionHandler.CreateJobAsync(A<EnrichedEvent>.Ignored, rule.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var job = await sut.CreateJobAsync(rule, ruleId, @event);

            Assert.Equal(actionData, job.ActionData);
            Assert.Equal(actionName, job.ActionName);
            Assert.Equal(actionDescription, job.Description);

            Assert.Equal(now, job.Created);
            Assert.Equal(now.Plus(Duration.FromDays(2)), job.Expires);

            Assert.Equal(enrichedEvent.AppId.Id, job.AppId);

            Assert.NotEqual(Guid.Empty, job.JobId);

            A.CallTo(() => eventEnricher.EnrichAsync(enrichedEvent, A<Envelope<AppEvent>>.That.Matches(x => x.Payload == @event.Payload)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_succeeded_job_with_full_dump_when_handler_returns_no_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10)))
                .Returns((actionDump, null));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Success, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Dump.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_failed_job_with_full_dump_when_handler_returns_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10)))
                .Returns((actionDump, new InvalidOperationException()));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Failed, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Dump.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_timedout_job_with_full_dump_when_exception_from_handler_indicates_timeout()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10)))
                .Returns((actionDump, new TimeoutException()));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Timeout, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Dump.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));

            Assert.True(result.Dump.IndexOf("Action timed out.", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task Should_create_exception_details_when_job_to_execute_failed()
        {
            var ruleError = new InvalidOperationException();

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10)))
                .Throws(ruleError);

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal((ruleError.ToString(), RuleResult.Failed, TimeSpan.Zero), result);
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
    }
}
