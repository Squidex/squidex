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
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IAssetLoader assetLoader = A.Fake<IAssetLoader>();
        private readonly IRuleTriggerHandler sut;

        public AssetChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
                .Returns(false);

            sut = new AssetChangedTriggerHandler(scriptEngine, assetLoader);
        }

        public static IEnumerable<object[]> TestEvents()
        {
            yield return new object[] { new AssetCreated(), EnrichedAssetEventType.Created };
            yield return new object[] { new AssetUpdated(), EnrichedAssetEventType.Updated };
            yield return new object[] { new AssetAnnotated(), EnrichedAssetEventType.Annotated };
            yield return new object[] { new AssetDeleted(), EnrichedAssetEventType.Deleted };
        }

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_create_enriched_events(AssetEvent @event, EnrichedAssetEventType type)
        {
            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            A.CallTo(() => assetLoader.GetAsync(@event.AssetId, 12))
                .Returns(new AssetEntity());

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            var enrichedEvent = result.Single() as EnrichedAssetEvent;

            Assert.Equal(type, enrichedEvent!.Type);
        }

        [Fact]
        public async Task Should_skip_moved_event()
        {
            var envelope = Envelope.Create<AppEvent>(new AssetMoved());

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Empty(result);
        }

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new ContentCreated(), trigger, Guid.NewGuid());

                Assert.False(result);
            });
        }

        [Fact]
        public void Should_trigger_precheck_when_event_type_correct()
        {
            TestForCondition(string.Empty, trigger =>
            {
                var result = sut.Trigger(new AssetCreated(), trigger, Guid.NewGuid());

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
                var result = sut.Trigger(new EnrichedAssetEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_trigger_check_when_condition_matchs()
        {
            TestForCondition("true", trigger =>
            {
                var result = sut.Trigger(new EnrichedAssetEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public void Should_not_trigger_check_when_condition_does_not_matchs()
        {
            TestForCondition("false", trigger =>
            {
                var result = sut.Trigger(new EnrichedAssetEvent(), trigger);

                Assert.False(result);
            });
        }

        private void TestForCondition(string condition, Action<AssetChangedTriggerV2> action)
        {
            var trigger = new AssetChangedTriggerV2 { Condition = condition };

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
