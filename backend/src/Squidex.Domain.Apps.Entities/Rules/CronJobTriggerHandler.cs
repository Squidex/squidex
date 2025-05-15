// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobTriggerHandler : IRuleTriggerHandler
{
    public Type TriggerType => typeof(CronJobTrigger);

    public bool Handles(AppEvent appEvent)
    {
        return appEvent is RuleCronJobTriggered;
    }

    public async IAsyncEnumerable<EnrichedEvent> CreateEnrichedEventsAsync(Envelope<AppEvent> @event, RulesContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var result = new EnrichedCronJobEvent();

        // Use the concrete event to map properties that are not part of app event.
        SimpleMapper.Map((RuleCronJobTriggered)@event.Payload, result);

        await Task.Yield();
        yield return result;
    }

    public string? GetName(AppEvent @event)
    {
        return "CronJob";
    }
}
