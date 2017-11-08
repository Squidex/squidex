// ==========================================================================
//  RuleTrigger.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleTrigger
    {
        public abstract T Accept<T>(IRuleTriggerVisitor<T> visitor);
    }
}
