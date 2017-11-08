// ==========================================================================
//  IRuleTriggerHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleTriggerHandler
    {
        Type TriggerType { get; }

        bool Triggers(Envelope<AppEvent> @event, RuleTrigger trigger);
    }
}
