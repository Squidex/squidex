// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public class UsageTriggerHandlerTests
    {
        private readonly DomainId ruleId = DomainId.NewGuid();
        private readonly IRuleTriggerHandler sut = new UsageTriggerHandler();

        [Fact]
        public void Should_return_false_when_asking_for_snapshot_support()
        {
            Assert.False(sut.CanCreateSnapshotEvents);
        }

        [Fact]
        public void Should_not_trigger_precheck_when_event_type_not_correct()
        {
            var result = sut.Trigger(new ContentCreated(), new UsageTrigger(), ruleId);

            Assert.False(result);
        }

        [Fact]
        public void Should_not_trigger_precheck_when_rule_id_not_matchs()
        {
            var result = sut.Trigger(new AppUsageExceeded { RuleId = DomainId.NewGuid() }, new UsageTrigger(), ruleId);

            Assert.True(result);
        }

        [Fact]
        public void Should_trigger_precheck_when_event_type_correct_and_rule_id_matchs()
        {
            var result = sut.Trigger(new AppUsageExceeded { RuleId = ruleId }, new UsageTrigger(), ruleId);

            Assert.True(result);
        }

        [Fact]
        public void Should_not_trigger_check_when_event_type_not_correct()
        {
            var result = sut.Trigger(new EnrichedContentEvent(), new UsageTrigger());

            Assert.False(result);
        }

        [Fact]
        public async Task Should_create_enriched_event()
        {
            var @event = new AppUsageExceeded { CallsCurrent = 80, CallsLimit = 120 };

            var result = await sut.CreateEnrichedEventsAsync(Envelope.Create<AppEvent>(@event));

            var enrichedEvent = result.Single() as EnrichedUsageExceededEvent;

            Assert.Equal(@event.CallsCurrent, enrichedEvent!.CallsCurrent);
            Assert.Equal(@event.CallsLimit, enrichedEvent!.CallsLimit);
        }
    }
}
