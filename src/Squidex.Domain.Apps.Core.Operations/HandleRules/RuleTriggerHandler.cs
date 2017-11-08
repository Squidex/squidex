// ==========================================================================
//  RuleTriggerHandler.cs
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
    public abstract class RuleTriggerHandler<T> : IRuleTriggerHandler where T : RuleTrigger
    {
        public Type TriggerType
        {
            get { return typeof(T); }
        }

        bool IRuleTriggerHandler.Triggers(Envelope<AppEvent> @event, RuleTrigger trigger)
        {
            return Triggers(@event, (T)trigger);
        }

        protected abstract bool Triggers(Envelope<AppEvent> @event, T trigger);
    }
}
