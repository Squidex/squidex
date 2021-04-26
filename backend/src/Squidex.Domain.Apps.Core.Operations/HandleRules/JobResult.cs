// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Rules;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed record JobResult(RuleJob? Job, Exception? Exception = null, SkipReason SkipReason = default)
    {
        public static readonly JobResult ConditionDoesNotMatch = new JobResult(null, null, SkipReason.ConditionDoesNotMatch);
        public static readonly JobResult Disabled = new JobResult(null, null, SkipReason.Disabled);
        public static readonly JobResult EventMismatch = new JobResult(null, null, SkipReason.EventMismatch);
        public static readonly JobResult FromRule = new JobResult(null, null, SkipReason.FromRule);
        public static readonly JobResult NoAction = new JobResult(null, null, SkipReason.NoAction);
        public static readonly JobResult NoTrigger = new JobResult(null, null, SkipReason.NoTrigger);
        public static readonly JobResult TooOld = new JobResult(null, null, SkipReason.TooOld);
        public static readonly JobResult WrongEventForTrigger = new JobResult(null, null, SkipReason.WrongEventForTrigger);
    }
}
