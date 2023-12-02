// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public interface IRuleEventEntity
{
    DomainId Id { get; }

    RuleJob Job { get; }

    Instant Created { get;  }

    Instant? NextAttempt { get; }

    RuleJobResult JobResult { get; }

    RuleResult Result { get; }

    int NumCalls { get; }

    string? LastDump { get; }
}
