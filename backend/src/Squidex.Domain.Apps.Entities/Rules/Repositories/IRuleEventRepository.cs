// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Rules.Repositories;

public record struct RuleEventWrite(RuleJob Job, Instant? NextAttempt = null, Exception? Error = null);

public interface IRuleEventRepository
{
    Task UpdateAsync(RuleJob job, RuleJobUpdate update,
        CancellationToken ct = default);

    Task EnqueueAsync(List<RuleEventWrite> jobs,
        CancellationToken ct = default);

    Task EnqueueAsync(DomainId id, Instant nextAttempt,
        CancellationToken ct = default);

    Task CancelByEventAsync(DomainId eventId,
        CancellationToken ct = default);

    Task CancelByRuleAsync(DomainId ruleId,
        CancellationToken ct = default);

    Task CancelByAppAsync(DomainId appId,
        CancellationToken ct = default);

    Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback,
        CancellationToken ct = default);

    Task<IResultList<IRuleEventEntity>> QueryByAppAsync(DomainId appId, DomainId? ruleId = null, int skip = 0, int take = 20,
        CancellationToken ct = default);

    Task<IRuleEventEntity> FindAsync(DomainId id,
        CancellationToken ct = default);
}
