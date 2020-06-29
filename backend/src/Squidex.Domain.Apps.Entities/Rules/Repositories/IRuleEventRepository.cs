// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public interface IRuleEventRepository
    {
        Task UpdateAsync(RuleJob job, RuleJobUpdate update);

        Task EnqueueAsync(RuleJob job, Instant? nextAttempt, CancellationToken ct = default);

        Task EnqueueAsync(DomainId id, Instant nextAttempt);

        Task CancelAsync(DomainId id);

        Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken ct = default);

        Task<IReadOnlyList<RuleStatistics>> QueryStatisticsByAppAsync(DomainId appId);

        Task<IResultList<IRuleEventEntity>> QueryByAppAsync(DomainId appId, DomainId? ruleId = null, int skip = 0, int take = 20);

        Task<IRuleEventEntity> FindAsync(DomainId id);
    }
}
