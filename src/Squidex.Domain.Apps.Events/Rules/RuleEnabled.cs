﻿// ==========================================================================
//  RuleEnabled.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Rules
{
    [EventType(nameof(RuleEnabled))]
    public sealed class RuleEnabled : RuleEvent
    {
    }
}
