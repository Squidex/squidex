// ==========================================================================
//  IRuleEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;

namespace Squidex.Domain.Apps.Read.Rules
{
    public interface IRuleEventEntity : IEntity
    {
        RuleJob Job { get; }

        Instant? NextAttempt { get; }

        RuleJobResult JobResult { get; }

        RuleResult Result { get; }

        int NumCalls { get; }

        string LastDump { get; }
    }
}
