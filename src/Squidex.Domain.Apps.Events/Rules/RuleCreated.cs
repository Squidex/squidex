// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
