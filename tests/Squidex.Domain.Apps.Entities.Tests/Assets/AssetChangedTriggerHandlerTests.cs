// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetChangedTriggerHandlerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IRuleTriggerHandler sut;

        public AssetChangedTriggerHandlerTests()
        {
            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
                .Returns(false);

            sut = new AssetChangedTriggerHandler(scriptEngine, grainFactory);
        }

        public static IEnumerable<object[]> TestEvents = new[]
        {
            new object[] { new AssetCreated(), EnrichedAssetEventType.Created },
            new object[] { new AssetUpdated(), EnrichedAssetEventType.Updated },
            new object[] { new AssetAnnotated(), EnrichedAssetEventType.Annotated },
            new object[] { new AssetDeleted(), EnrichedAssetEventType.Deleted }
        };

        [Theory]
        [MemberData(nameof(TestEvents))]
        public async Task Should_enrich_events(AssetEvent @event, EnrichedAssetEventType type)
        {
            var envelope = Envelope.Create<AppEvent>(@event).SetEventStreamNumber(12);

            var assetGrain = A.Fake<IAssetGrain>();

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(@event.AssetId, null))
                .Returns(assetGrain);

            A.CallTo(() => assetGrain.GetStateAsync(12))
                .Returns(J.Of<IAssetEntity>(new AssetEntity()));

            var result = await sut.CreateEnrichedEventAsync(envelope);

            Assert.Equal(type, ((EnrichedAssetEvent)result).Type);
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
