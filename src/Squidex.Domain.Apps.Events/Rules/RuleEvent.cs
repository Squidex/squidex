// ==========================================================================
//  RuleEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Rules
{
    public abstract class RuleEvent : AppEvent
    {
        public Guid RuleId { get; set; }
    }
}
