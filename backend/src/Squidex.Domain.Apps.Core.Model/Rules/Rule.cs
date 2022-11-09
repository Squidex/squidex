// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules;

public sealed class Rule
{
    public string? Name { get; private set; }

    public RuleTrigger Trigger { get; private set; }

    public RuleAction Action { get; private set; }

    public bool IsEnabled { get; private set; } = true;

    public Rule(RuleTrigger trigger, RuleAction action)
    {
        Guard.NotNull(trigger);
        Guard.NotNull(action);

        Action = action;

        Trigger = trigger;
    }

    [Pure]
    public Rule Rename(string newName)
    {
        if (string.Equals(Name, newName, StringComparison.Ordinal))
        {
            return this;
        }

        return Clone(clone =>
        {
            clone.Name = newName;
        });
    }

    [Pure]
    public Rule Enable()
    {
        if (IsEnabled)
        {
            return this;
        }

        return Clone(clone =>
        {
            clone.IsEnabled = true;
        });
    }

    [Pure]
    public Rule Disable()
    {
        if (!IsEnabled)
        {
            return this;
        }

        return Clone(clone =>
        {
            clone.IsEnabled = false;
        });
    }

    [Pure]
    public Rule Update(RuleTrigger newTrigger)
    {
        Guard.NotNull(newTrigger);

        if (newTrigger.GetType() != Trigger.GetType())
        {
            ThrowHelper.ArgumentException("New trigger has another type.", nameof(newTrigger));
        }

        if (Trigger.Equals(newTrigger))
        {
            return this;
        }

        return Clone(clone =>
        {
            clone.Trigger = newTrigger;
        });
    }

    [Pure]
    public Rule Update(RuleAction newAction)
    {
        Guard.NotNull(newAction);

        if (newAction.GetType() != Action.GetType())
        {
            ThrowHelper.ArgumentException("New action has another type.", nameof(newAction));
        }

        if (Action.Equals(newAction))
        {
            return this;
        }

        return Clone(clone =>
        {
            clone.Action = newAction;
        });
    }

    private Rule Clone(Action<Rule> updater)
    {
        var clone = (Rule)MemberwiseClone();

        updater(clone);

        return clone;
    }
}
