// ==========================================================================
//  RuleDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Rules
{
    [EventType(nameof(RuleDeleted))]
    public sealed class RuleDeleted : RuleEvent
    {
    }
}
