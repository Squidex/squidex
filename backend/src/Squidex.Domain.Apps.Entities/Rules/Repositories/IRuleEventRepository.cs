// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public interface IRuleEventRepository
    {
        async Task EnqueueAsync(RuleJob job, Exception? ex)
        {
            if (ex != null)
            {
                await EnqueueAsync(job, (Instant?)null);

                await UpdateAsync(job, new RuleJobUpdate
                {
                    JobResult = RuleJobResult.Failed,
                    ExecutionResult = RuleResult.Failed,
                    ExecutionDump = ex.ToString(),
                    Finished = job.Created
                });
            }
            else
            {
                await EnqueueAsync(job, job.Created);
            }
        }

        Task UpdateAsync(RuleJob job, RuleJobUpdate update);

        Task EnqueueAsync(RuleJob job, Instant? nextAttempt);

        Task EnqueueAsync(DomainId id, Instant nextAttempt);

        Task CancelAsync(DomainId id);

        Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken ct = default);

        Task<IReadOnlyList<RuleStatistics>> QueryStatisticsByAppAsync(DomainId appId, CancellationToken ct = default);

        Task<IResultList<IRuleEventEntity>> QueryByAppAsync(DomainId appId, DomainId? ruleId = null, int skip = 0, int take = 20, CancellationToken ct = default);

        Task<IRuleEventEntity> FindAsync(DomainId id, CancellationToken ct = default);
    }
}
