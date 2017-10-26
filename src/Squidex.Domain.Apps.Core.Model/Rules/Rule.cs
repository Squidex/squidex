// ==========================================================================
//  Rule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public sealed class Rule
    {
        private RuleTrigger trigger;
        private RuleAction action;
        private bool isEnabled = true;

        public RuleTrigger Trigger
        {
            get { return trigger; }
        }

        public RuleAction Action
        {
            get { return action; }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
        }

        public Rule(RuleTrigger trigger, RuleAction action)
        {
            Guard.NotNull(trigger, nameof(trigger));
            Guard.NotNull(action, nameof(action));

            this.trigger = trigger;
            this.action = action;
        }

        public void Enable()
        {
            this.isEnabled = true;
        }

        public void Disable()
        {
            this.isEnabled = false;
        }

        public void Update(RuleTrigger newTrigger)
        {
            Guard.NotNull(newTrigger, nameof(newTrigger));

            if (newTrigger.GetType() != trigger.GetType())
            {
                throw new ArgumentException("New trigger has another type.", nameof(newTrigger));
            }

            trigger = newTrigger;
        }

        public void Update(RuleAction newAction)
        {
            Guard.NotNull(newAction, nameof(newAction));

            if (newAction.GetType() != trigger.GetType())
            {
                throw new ArgumentException("New action has another type.", nameof(newAction));
            }

            action = newAction;
        }
    }
}
