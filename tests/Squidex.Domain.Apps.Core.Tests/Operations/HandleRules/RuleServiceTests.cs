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
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
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
            public int Value { get; set; }
        }

        public sealed class ValidData
        {
            public int Value { get; set; }
        }

        public sealed class InvalidTrigger : RuleTrigger
        {
            public override T Accept<T>(IRuleTriggerVisitor<T> visitor)
            {
                return default(T);
            }
        }

        public RuleServiceTests()
        {
            typeNameRegistry.Map(typeof(ContentCreated));
            typeNameRegistry.Map(typeof(ValidAction), actionName);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .Returns(new EnrichedContentEvent { AppId = appId });

            A.CallTo(() => ruleActionHandler.ActionType)
                .Returns(typeof(ValidAction));

            A.CallTo(() => ruleActionHandler.DataType)
                .Returns(typeof(ValidData));

            A.CallTo(() => ruleTriggerHandler.TriggerType)
                .Returns(typeof(ContentChangedTriggerV2));

            sut = new RuleService(new[] { ruleTriggerHandler }, new[] { ruleActionHandler }, eventEnricher, TestUtils.DefaultSerializer, clock, typeNameRegistry);
        }

        [Fact]
        public async Task Should_not_create_if_rule_disabled()
        {
            var ruleConfig = ValidRule().Disable();
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_for_invalid_event()
        {
            var ruleConfig = ValidRule();
            var ruleEnvelope = Envelope.Create(new InvalidEvent());

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_trigger_handler_registered()
        {
            var ruleConfig = new Rule(new InvalidTrigger(), new ValidAction());
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_no_action_handler_registered()
        {
            var ruleConfig = new Rule(new ContentChangedTriggerV2(), new InvalidAction());
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered_with_precheck()
        {
            var ruleConfig = ValidRule();
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(A<IEvent>.Ignored, ruleConfig.Trigger))
                .Returns(false);

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_create_job_if_not_triggered()
        {
            var ruleConfig = ValidRule();
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Trigger(A<IEvent>.Ignored, ruleConfig.Trigger))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>.Ignored, ruleConfig.Trigger))
                .Returns(false);

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public async Task Should_not_create_job_if_too_old()
        {
            var @event = new ContentCreated { SchemaId = schemaId, AppId = appId };

            var now = SystemClock.Instance.GetCurrentInstant();

            var ruleConfig = ValidRule();
            var ruleEnvelope = Envelope.Create(@event);

            ruleEnvelope.SetTimestamp(now.Minus(Duration.FromDays(3)));

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => ruleActionHandler.CreateJobAsync(A<EnrichedEvent>.Ignored, ruleConfig.Action))
                .Returns((actionDescription, actionData));

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Null(job);

            A.CallTo(() => eventEnricher.EnrichAsync(A<Envelope<AppEvent>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_create_job_if_triggered()
        {
            var @event = new ContentCreated { SchemaId = schemaId, AppId = appId };

            var now = Instant.FromUnixTimeSeconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeSeconds());

            var ruleConfig = ValidRule();
            var ruleEnvelope = Envelope.Create(@event);

            ruleEnvelope.SetTimestamp(now);

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<EnrichedEvent>.Ignored, ruleConfig.Trigger))
                .Returns(true);

            A.CallTo(() => ruleTriggerHandler.Trigger(A<IEvent>.Ignored, ruleConfig.Trigger))
                .Returns(true);

            A.CallTo(() => ruleActionHandler.CreateJobAsync(A<EnrichedEvent>.Ignored, ruleConfig.Action))
                .Returns((actionDescription, new ValidData { Value = 10 }));

            var job = await sut.CreateJobAsync(ruleConfig, ruleEnvelope);

            Assert.Equal(actionData, job.ActionData);
            Assert.Equal(actionName, job.ActionName);
            Assert.Equal(actionDescription, job.Description);

            Assert.Equal(now, job.Created);
            Assert.Equal(now.Plus(Duration.FromDays(2)), job.Expires);

            Assert.Equal(@event.AppId.Id, job.AppId);

            Assert.NotEqual(Guid.Empty, job.JobId);
        }

        [Fact]
        public async Task Should_return_succeeded_job_with_full_dump_when_handler_returns_no_exception()
        {
            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(A<ValidData>.That.Matches(x => x.Value == 10)))
                .Returns((actionDump, null));

            var result = await sut.InvokeAsync(actionName, actionData);

            Assert.Equal(RuleResult.Success, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.StartsWith(actionDump, result.Dump, StringComparison.OrdinalIgnoreCase);
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

        private static Rule ValidRule()
        {
            return new Rule(new ContentChangedTriggerV2(), new ValidAction());
        }
    }
}
