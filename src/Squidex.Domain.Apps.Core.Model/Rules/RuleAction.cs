// ==========================================================================
//  RuleAction.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleAction
    {
        public abstract T Accept<T>(IRuleActionVisitor<T> visitor);
    }
}
