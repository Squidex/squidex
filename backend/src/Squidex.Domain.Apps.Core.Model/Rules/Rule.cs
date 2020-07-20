// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public sealed class Rule : Cloneable<Rule>
    {
        private RuleTrigger trigger;
        private RuleAction action;
        private string name;
        private bool isEnabled = true;

        public string Name
        {
            get { return name; }
        }

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

            SetTrigger(trigger);
            SetAction(action);
        }

        [Pure]
        public Rule Rename(string newName)
        {
            if (string.Equals(name, newName))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.name = newName;
            });
        }

        [Pure]
        public Rule Enable()
        {
            if (isEnabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isEnabled = true;
            });
        }

        [Pure]
        public Rule Disable()
        {
            if (!isEnabled)
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.isEnabled = false;
            });
        }

        [Pure]
        public Rule Update(RuleTrigger newTrigger)
        {
            Guard.NotNull(newTrigger, nameof(newTrigger));

            if (newTrigger.GetType() != trigger.GetType())
            {
                throw new ArgumentException("New trigger has another type.", nameof(newTrigger));
            }

            if (trigger.DeepEquals(newTrigger))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.SetTrigger(newTrigger);
            });
        }

        [Pure]
        public Rule Update(RuleAction newAction)
        {
            Guard.NotNull(newAction, nameof(newAction));

            if (newAction.GetType() != action.GetType())
            {
                throw new ArgumentException("New action has another type.", nameof(newAction));
            }

            if (action.DeepEquals(newAction))
            {
                return this;
            }

            return Clone(clone =>
            {
                clone.SetAction(newAction);
            });
        }

        private void SetAction(RuleAction newAction)
        {
            action = newAction;
            action.Freeze();
        }

        private void SetTrigger(RuleTrigger newTrigger)
        {
            trigger = newTrigger;
            trigger.Freeze();
        }
    }
}
