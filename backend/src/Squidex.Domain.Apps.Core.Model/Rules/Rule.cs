﻿// ==========================================================================
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
            Guard.NotNull(trigger);
            Guard.NotNull(action);

            this.trigger = trigger;
            this.trigger.Freeze();

            this.action = action;
            this.action.Freeze();
        }

        [Pure]
        public Rule Rename(string newName)
        {
            return Clone(clone =>
            {
                clone.name = newName;
            });
        }

        [Pure]
        public Rule Enable()
        {
            return Clone(clone =>
            {
                clone.isEnabled = true;
            });
        }

        [Pure]
        public Rule Disable()
        {
            return Clone(clone =>
            {
                clone.isEnabled = false;
            });
        }

        [Pure]
        public Rule Update(RuleTrigger newTrigger)
        {
            Guard.NotNull(newTrigger);

            if (newTrigger.GetType() != trigger.GetType())
            {
                throw new ArgumentException("New trigger has another type.", nameof(newTrigger));
            }

            newTrigger.Freeze();

            return Clone(clone =>
            {
                clone.trigger = newTrigger;
            });
        }

        [Pure]
        public Rule Update(RuleAction newAction)
        {
            Guard.NotNull(newAction);

            if (newAction.GetType() != action.GetType())
            {
                throw new ArgumentException("New action has another type.", nameof(newAction));
            }

            newAction.Freeze();

            return Clone(clone =>
            {
                clone.action = newAction;
            });
        }
    }
}
