// ==========================================================================
//  IRuleTriggerVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleTriggerVisitor<out T>
    {
        T Visit(ContentChangedTrigger trigger);
    }
}
