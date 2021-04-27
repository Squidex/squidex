// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public enum SkipReason
    {
        None,
        ConditionDoesNotMatch,
        Disabled,
        EventMismatch,
        Failed,
        FromRule,
        NoAction,
        NoTrigger,
        TooOld,
        WrongEventForTrigger
    }
}