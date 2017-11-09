﻿// ==========================================================================
//  RuleJobResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read.Rules
{
    public enum RuleJobResult
    {
        Pending,
        Success,
        Retry,
        Failed
    }
}
