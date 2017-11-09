// ==========================================================================
//  RuleEventDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Events.Rules.Utils
{
    public static class RuleEventDispatcher
    {
        public static Rule Create(RuleCreated @event)
        {
            return new Rule(@event.Trigger, @event.Action);
        }

        public static void Apply(this Rule rule, RuleUpdated @event)
        {
            if (@event.Trigger != null)
            {
                rule.Update(@event.Trigger);
            }

            if (@event.Action != null)
            {
                rule.Update(@event.Action);
            }
        }

        public static void Apply(this Rule rule, RuleEnabled @event)
        {
            rule.Enable();
        }

        public static void Apply(this Rule rule, RuleDisabled @event)
        {
            rule.Disable();
        }
    }
}
