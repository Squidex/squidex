// ==========================================================================
//  RuleResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public enum RuleResult
    {
        Pending,
        Success,
        Failed,
        Timeout
    }
}
