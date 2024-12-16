// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed partial class RuleService
{
    private sealed class RuleStates : List<RuleState>
    {
        public RuleStates()
        {
        }

        public RuleStates(IEnumerable<RuleState> source)
            : base(source)
        {
        }
    }

    private sealed class RuleState(Rule rule)
    {
        public Rule Rule { get; } = rule;

        public SkipReason Skip { get; set; }
    }

    private string GetName(AppEvent @event)
    {
        foreach (var (_, handler) in ruleTriggerHandlers)
        {
            if (handler.Handles(@event))
            {
                var name = handler.GetName(@event);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
        }

        return @event.GetType().Name;
    }
}
