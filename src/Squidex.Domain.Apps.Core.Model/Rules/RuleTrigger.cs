// ==========================================================================
//  RuleTrigger.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleTrigger : Freezable
    {
        public abstract T Accept<T>(IRuleTriggerVisitor<T> visitor);
    }
}
