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
        Failed,
        FromRule,
        NoAction,
        NoTrigger,
        TooOld,
        WrongEvent,
        WrongEventForTrigger
    }
}
