// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
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

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "true"))
                .Returns(true);

            A.CallTo(() => scriptEngine.Evaluate("event", A<object>.Ignored, "false"))
                .Returns(false);
        }

        [Fact]
        public Task Should_not_trigger_precheck_when_event_type_not_correct()
        {
            return TestForConditionAsync(string.Empty, async trigger =>
            {
                var result = await sut.TriggersAsync(new ContentCreated(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public Task Should_trigger_precheck_when_event_type_correct()
        {
            return TestForConditionAsync(string.Empty, async trigger =>
            {
                var result = await sut.TriggersAsync(new AssetCreated(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public Task Should_not_trigger_check_when_event_type_not_correct()
        {
            return TestForConditionAsync(string.Empty, async trigger =>
            {
                var result = await sut.TriggersAsync(new EnrichedContentEvent(), trigger);

                Assert.False(result);
            });
        }

        [Fact]
        public Task Should_trigger_check_when_condition_is_empty()
        {
            return TestForConditionAsync(string.Empty, async trigger =>
            {
                var result = await sut.TriggersAsync(new EnrichedAssetEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public Task Should_trigger_check_when_condition_matchs()
        {
            return TestForConditionAsync("true", async trigger =>
            {
                var result = await sut.TriggersAsync(new EnrichedAssetEvent(), trigger);

                Assert.True(result);
            });
        }

        [Fact]
        public Task Should_not_trigger_check_when_condition_does_not_matchs()
        {
            return TestForConditionAsync("false", async trigger =>
            {
                var result = await sut.TriggersAsync(new EnrichedAssetEvent(), trigger);

                Assert.False(result);
            });
        }

        private async Task TestForConditionAsync(string condition, Func<AssetChangedTriggerV2, Task> action)
        {
            var trigger = new AssetChangedTriggerV2 { Condition = condition };

            await action(trigger);

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
