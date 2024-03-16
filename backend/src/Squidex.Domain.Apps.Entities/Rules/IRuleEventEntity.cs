// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public interface IRuleEventEntity : IEntity
    {
        RuleJob Job { get; }

        Instant? NextAttempt { get; }

        RuleJobResult JobResult { get; }

        RuleResult Result { get; }

        int NumCalls { get; }

        string? LastDump { get; }
    }
}
