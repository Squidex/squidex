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
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Actions;
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
        private readonly IClock clock = A.Fake<IClock>();
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();
        private readonly RuleService sut;

        public sealed class InvalidEvent : IEvent
        {
        }

        public sealed class InvalidAction : RuleAction
        {
            public override T Accept<T>(IRuleActionVisitor<T> visitor)
            {
                return default(T);
            }
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
            typeNameRegistry.Map(typeof(WebhookAction));

            A.CallTo(() => ruleActionHandler.ActionType)
                .Returns(typeof(WebhookAction));

            A.CallTo(() => ruleTriggerHandler.TriggerType)
                .Returns(typeof(ContentChangedTrigger));

            sut = new RuleService(new[] { ruleTriggerHandler }, new[] { ruleActionHandler }, clock, typeNameRegistry);
        }

        [Fact]
        public void Should_not_create_job_for_invalid_event()
        {
            var ruleConfig = new Rule(new ContentChangedTrigger(), new WebhookAction());
            var ruleEnvelope = Envelope.Create(new InvalidEvent());

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public void Should_not_create_trigger_if_no_trigger_handler_registered()
        {
            var ruleConfig = new Rule(new InvalidTrigger(), new WebhookAction());
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public void Should_not_create_trigger_if_no_action_handler_registered()
        {
            var ruleConfig = new Rule(new ContentChangedTrigger(), new InvalidAction());
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public void Should_not_create_if_not_triggered()
        {
            var ruleConfig = new Rule(new ContentChangedTrigger(), new InvalidAction());
            var ruleEnvelope = Envelope.Create(new ContentCreated());

            A.CallTo(() => ruleTriggerHandler.Triggers(A<Envelope<AppEvent>>.Ignored, ruleConfig.Trigger))
                .Returns(false);

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public void Should_not_create_job_if_too_old()
        {
            var e = new ContentCreated { SchemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema"), AppId = new NamedId<Guid>(Guid.NewGuid(), "my-event") };

            var now = SystemClock.Instance.GetCurrentInstant();

            var ruleConfig = new Rule(new ContentChangedTrigger(), new WebhookAction());
            var ruleEnvelope = Envelope.Create(e);

            ruleEnvelope.SetTimestamp(now.Minus(Duration.FromDays(3)));

            var actionData = new RuleJobData();
            var actionDescription = "MyDescription";

            var eventName = "MySchemaCreatedEvent";

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => ruleTriggerHandler.Triggers(A<Envelope<AppEvent>>.Ignored, ruleConfig.Trigger))
                .Returns(true);

            A.CallTo(() => ruleActionHandler.CreateJob(A<Envelope<AppEvent>>.Ignored, eventName, ruleConfig.Action))
                .Returns((actionDescription, actionData));

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Null(job);
        }

        [Fact]
        public void Should_create_job_if_triggered()
        {
            var e = new ContentCreated { SchemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema"), AppId = new NamedId<Guid>(Guid.NewGuid(), "my-event") };

            var now = SystemClock.Instance.GetCurrentInstant();

            var ruleConfig = new Rule(new ContentChangedTrigger(), new WebhookAction());
            var ruleEnvelope = Envelope.Create(e);

            ruleEnvelope.SetTimestamp(now);

            var actionName = "WebhookAction";
            var actionData = new RuleJobData();
            var actionDescription = "MyDescription";

            var eventName = "MySchemaCreatedEvent";

            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(now);

            A.CallTo(() => ruleTriggerHandler.Triggers(A<Envelope<AppEvent>>.Ignored, ruleConfig.Trigger))
                .Returns(true);

            A.CallTo(() => ruleActionHandler.CreateJob(A<Envelope<AppEvent>>.Ignored, eventName, ruleConfig.Action))
                .Returns((actionDescription, actionData));

            var job = sut.CreateJob(ruleConfig, ruleEnvelope);

            Assert.Equal(eventName, job.EventName);

            Assert.Equal(actionData, job.ActionData);
            Assert.Equal(actionName, job.ActionName);
            Assert.Equal(actionDescription, job.Description);

            Assert.Equal(now, job.Created);
            Assert.Equal(now.Plus(Duration.FromDays(2)), job.Expires);

            Assert.Equal(e.AppId.Id, job.AppId);

            Assert.NotEqual(Guid.Empty, job.JobId);
        }

        [Fact]
        public async Task Should_return_succeeded_job_with_full_dump_when_handler_returns_no_exception()
        {
            var ruleJob = new RuleJobData();
            var ruleEx = new InvalidOperationException();

            var actionDump = "MyDump";

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(ruleJob))
                .Returns((actionDump, null));

            var result = await sut.InvokeAsync("WebhookAction", ruleJob);

            Assert.Equal(RuleResult.Success, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.StartsWith(actionDump, result.Dump, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Should_return_failed_job_with_full_dump_when_handler_returns_exception()
        {
            var ruleJob = new RuleJobData();
            var ruleEx = new InvalidOperationException();

            var actionDump = "MyDump";

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(ruleJob))
                .Returns((actionDump, new InvalidOperationException()));

            var result = await sut.InvokeAsync("WebhookAction", ruleJob);

            Assert.Equal(RuleResult.Failed, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Dump.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Should_return_timedout_job_with_full_dump_when_exception_from_handler_indicates_timeout()
        {
            var ruleJob = new RuleJobData();
            var ruleEx = new InvalidOperationException();

            var actionDump = "MyDump";

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(ruleJob))
                .Returns((actionDump, new TimeoutException()));

            var result = await sut.InvokeAsync("WebhookAction", ruleJob);

            Assert.Equal(RuleResult.Timeout, result.Result);

            Assert.True(result.Elapsed >= TimeSpan.Zero);
            Assert.True(result.Dump.StartsWith(actionDump, StringComparison.OrdinalIgnoreCase));
            Assert.True(result.Dump.IndexOf("Action timed out.", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task Should_create_exception_details_when_job_to_execute_failed()
        {
            var ruleJob = new RuleJobData();
            var ruleEx = new InvalidOperationException();

            A.CallTo(() => ruleActionHandler.ExecuteJobAsync(ruleJob))
                .Throws(ruleEx);

            var result = await sut.InvokeAsync("WebhookAction", ruleJob);

            Assert.Equal((ruleEx.ToString(), RuleResult.Failed, TimeSpan.Zero), result);
        }
    }
}