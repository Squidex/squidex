// ==========================================================================
//  RuleDisabled.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Rules
{
    [EventType(nameof(RuleDisabled))]
    public sealed class RuleDisabled : RuleEvent
    {
    }
}
