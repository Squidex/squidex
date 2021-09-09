// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    [Flags]
    public enum SkipReason
    {
        None = 0,
        ConditionDoesNotMatch = 1 << 0,
        ConditionPrecheckDoesNotMatch = 1 << 1,
        Disabled = 1 << 2,
        Failed = 1 << 3,
        FromRule = 1 << 4,
        NoAction = 1 << 5,
        NoTrigger = 1 << 6,
        TooOld = 1 << 7,
        WrongEvent = 1 << 8,
        WrongEventForTrigger = 1 << 9
    }
}
