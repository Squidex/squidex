// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed record SimulatedRuleEvent(
        string EventName,
        string? ActionName,
        string? ActionData,
        string? Error,
        SkipReason SkipReason)
    {
    }
}
