// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public class SchemaChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IRuleTriggerHandler sut;

        public SchemaChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
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

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(SchemaEvent @event, EnrichedSchemaEventType type)
        {
            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            var enrichedEvent = result.Single() as EnrichedSchemaEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new AppCreated(), trigger, Guid.NewGuid());

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_event_type_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new SchemaCreated(), trigger, Guid.NewGuid());

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_event_type_not_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new EnrichedContentEvent(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_is_empty()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new EnrichedSchemaEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_matchs()
        {
            TestForCondition("true", trigger =>
            {
                var result = sut.Trigger(new EnrichedSchemaEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_condition_does_not_matchs()
        {
            TestForCondition("false", trigger =>
            {
                var result = sut.Trigger(new EnrichedSchemaEvent(), trigger);

                Assert.False(result);
            });
        }

        private void TestForCondition(string condition, Action<SchemaChangedTrigger> action)
        {
            var trigger = new SchemaChangedTrigger { Condition = condition };

            action(trigger);

            if (string.IsNullOrWhiteSpace(condition))
            {
                A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, condition))
                    .MustNotHaveHappened();
            }
            else
            {
                A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, condition))
                    .MustHaveHappened();
            }
        }
    }
}
