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

        public static Rule Apply(this Rule rule, RuleUpdated @event)
        {
            if (@event.Trigger != null)
            {
                return rule.Update(@event.Trigger);
            }

            if (@event.Action != null)
            {
                return rule.Update(@event.Action);
            }

            return rule;
        }

        public static Rule Apply(this Rule rule, RuleEnabled @event)
        {
            return rule.Enable();
        }

        public static Rule Apply(this Rule rule, RuleDisabled @event)
        {
            return rule.Disable();
        }
    }
}
