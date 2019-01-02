// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class AssetChangedTriggerTests
    {
        private readonly IScriptEngine scriptEngine = A.Fake<IScriptEngine>();
        private readonly IRuleTriggerHandler sut;

        public AssetChangedTriggerTests()
        {
            sut = new AssetChangedTriggerHandler(scriptEngine);
        }

        [Fact]
        public void Should_trigger_when_condition_is_null()
        {
            var trigger = new AssetChangedTriggerV2();

            var result = sut.Triggers(new EnrichedAssetEvent(), trigger);

            Assert.True(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_condition_is_empty()
        {
            var trigger = new AssetChangedTriggerV2 { Condition = string.Empty };

            var result = sut.Triggers(new EnrichedAssetEvent(), trigger);

            Assert.True(result);

            A.CallTo(() => scriptEngine.Evaluate(A<string>.Ignored, A<object>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_trigger_when_condition_matchs()
        {
            var trigger = new AssetChangedTriggerV2 { Condition = "true" };

            var @event = new EnrichedAssetEvent();

            A.CallTo(() => scriptEngine.Evaluate("event", @event, trigger.Condition))
                .Returns(true);

            var result = sut.Triggers(@event, trigger);

            Assert.True(result);
        }

        [Fact]
        public void Should_not_trigger_when_condition_does_not_matchs()
        {
            var trigger = new AssetChangedTriggerV2 { Condition = "false" };

            var @event = new EnrichedAssetEvent();

            A.CallTo(() => scriptEngine.Evaluate("event", @event, trigger.Condition))
                .Returns(false);

            var result = sut.Triggers(@event, trigger);

            Assert.False(result);
        }
    }
}
