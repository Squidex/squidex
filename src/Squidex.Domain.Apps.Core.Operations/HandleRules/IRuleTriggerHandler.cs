// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleTriggerHandler
    {
        Type TriggerType { get; }

        Task<EnrichedEvent> CreateEnrichedEventAsync(Envelope<AppEvent> @event);

        bool Trigger(EnrichedEvent @event, RuleTrigger trigger);

        bool Trigger(AppEvent @event, RuleTrigger trigger, Guid ruleId);
    }
}
