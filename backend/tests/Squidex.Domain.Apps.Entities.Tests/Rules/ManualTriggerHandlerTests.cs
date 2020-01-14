﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class ManualTriggerHandlerTests
    {
        private readonly IRuleTriggerHandler sut = new ManualTriggerHandler();

        [Fact]
        public async Task Should_create_event_with_name()
        {
            var envelope = Envelope.Create<AppEvent>(new RuleManuallyTriggered());

            var result = await sut.CreateEnrichedEventsAsync(envelope);

            Assert.Equal("Manual", result.Single().Name);
        }

        [Fact]
        public void Should_always_trigger()
        {
            Assert.True(sut.Trigger(new EnrichedManualEvent(), new ManualTrigger()));
        }
    }
}
