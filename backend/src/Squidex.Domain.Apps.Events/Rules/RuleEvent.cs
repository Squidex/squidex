// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Rules
{
    public abstract class RuleEvent : AppEvent
    {
        public Guid RuleId { get; set; }
    }
}
