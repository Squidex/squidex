// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class ManualTriggerHandlerTests
    {
        private readonly IRuleTriggerHandler sut = new ManualTriggerHandler();

        [Fact]
        public void Should_return_false_if_asking_for_snapshot_support()
        {
            Assert.False(sut.CanCreateSnapshotEvents);
        }

        [Fact]
        public void Should_calculate_name()
        {
            var @event = new RuleManuallyTriggered();

            Assert.Equal("Manual", sut.GetName(@event));
        }

        [Fact]
        public async Task Should_create_event_with_name()
        {
            var @event = new RuleManuallyTriggered();

            var result = await sut.CreateEnrichedEventsAsync(Envelope.Create<AppEvent>(@event), default, default).ToListAsync();

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Should_create_event_with_actor()
        {
            var actor = RefToken.User("me");

            var @event = new RuleManuallyTriggered { Actor = actor };

            var result = await sut.CreateEnrichedEventsAsync(Envelope.Create<AppEvent>(@event), default, default).ToListAsync();

            Assert.Equal(actor, ((EnrichedUserEventBase)result.Single()).Actor);
        }

        [Fact]
        public void Should_always_trigger()
        {
            var @event = new RuleManuallyTriggered();

            Assert.True(sut.Trigger(Envelope.Create<AppEvent>(@event), default));
        }

        [Fact]
        public void Should_always_trigger_enriched_event()
        {
            var @event = new EnrichedUsageExceededEvent();

            Assert.True(sut.Trigger(@event, default));
        }
    }
}
