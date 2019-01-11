// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.HandleRules.Triggers;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules.Triggers
{
    public class UsageTriggerHandlerTests
    {
        private readonly IRuleTriggerHandler sut = new UsageTriggerHandler();

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            var result = sut.Trigger(new ContentCreated(), new UsageTrigger());

            Assert.False(result);
        }

        [Fact]
        public void Should_trigger_precheck_when_event_type_correct()
        {
            var result = sut.Trigger(new AppUsageExceeded(), new UsageTrigger());

            Assert.True(result);
        }

        [Fact]
        public void Should_not_trigger_check_when_event_type_not_correct()
        {
            var result = sut.Trigger(new EnrichedContentEvent(), new UsageTrigger());

            Assert.False(result);
        }

        [Fact]
        public void Should_trigger_check_when_type_correct()
        {
            var result = sut.Trigger(new AppUsageExceeded(), new UsageTrigger());

            Assert.True(result);
        }
    }
}
