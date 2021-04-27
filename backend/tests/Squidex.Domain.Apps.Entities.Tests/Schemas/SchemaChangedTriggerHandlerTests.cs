// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IRuleTriggerHandler sut;

        public SchemaChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "true", default))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, "false", default))
                .Returns(false);

            sut = new SchemaChangedTriggerHandler(scriptEngine);
        }

        public static IEnumerable<object[]> TestEvents()
        {
            yield return new object[] { new SchemaCreated(), EnrichedSchemaEventType.Created };
            yield return new object[] { new SchemaUpdated(), EnrichedSchemaEventType.Updated };
            yield return new object[] { new SchemaDeleted(), EnrichedSchemaEventType.Deleted };
            yield return new object[] { new SchemaPublished(), EnrichedSchemaEventType.Published };
            yield return new object[] { new SchemaUnpublished(), EnrichedSchemaEventType.Unpublished };
        }

        [Fact]
        public void Should_return_false_if_asking_for_snapshot_support()
        {
            Assert.False(sut.CanCreateSnapshotEvents);
        }

        [Fact]
        public void Should_handle_schema_event()
        {
            Assert.True(sut.Handles(new SchemaCreated()));
        }

        [Fact]
        public void Should_not_handle_other_event()
        {
            Assert.False(sut.Handles(new AppCreated()));
        }

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(SchemaEvent @event, EnrichedSchemaEventType type)
        {
            var ctx = Context();

            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var result = await sut.CreateEnrichedEventsAsync(envelope, ctx, default).ToListAsync();

            var enrichedEvent = result.Single() as EnrichedSchemaEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public void Should_trigger_precheck_if_event_type_correct()
        {
            TestForCondition(string.Empty, ctx =>
            {
                var @event = new SchemaCreated();

                var result = sut.Trigger(Envelope.Create<AppEvent>(@event), ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_if_condition_is_empty()
        {
            TestForCondition(string.Empty, ctx =>
            {
                var @event = new EnrichedSchemaEvent();

                var result = sut.Trigger(@event, ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_if_condition_matchs()
        {
            TestForCondition("true", ctx =>
            {
                var @event = new EnrichedSchemaEvent();

                var result = sut.Trigger(@event, ctx);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_if_condition_does_not_match()
        {
            TestForCondition("false", ctx =>
            {
                var @event = new EnrichedSchemaEvent();

                var result = sut.Trigger(@event, ctx);

                Assert.False(result);
            });
        }

        private void TestForCondition(string condition, Action<RuleContext> action)
        {
            var trigger = new SchemaChangedTrigger
            {
                Condition = condition
            };

            action(Context(trigger));

            if (string.IsNullOrWhiteSpace(condition))
            {
                A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                    .MustNotHaveHappened();
            }
            else
            {
                A.CallTo(() => scriptEngine.Evaluate(A<ScriptVars>._, condition, default))
                    .MustHaveHappened();
            }
        }

        private static RuleContext Context(RuleTrigger? trigger = null)
        {
            trigger ??= new SchemaChangedTrigger();

            return new RuleContext
            {
                AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
                Rule = new Rule(trigger, A.Fake<RuleAction>()),
                RuleId = DomainId.NewGuid()
            };
        }
    }
}
