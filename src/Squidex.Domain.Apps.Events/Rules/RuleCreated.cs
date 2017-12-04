// ==========================================================================
//  RuleCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Rules
{
    [EventType(nameof(RuleCreated))]
    public sealed class RuleCreated : RuleEvent
    {
        public RuleTrigger Trigger { get; set; }

        public RuleAction Action { get; set; }
    }
}
