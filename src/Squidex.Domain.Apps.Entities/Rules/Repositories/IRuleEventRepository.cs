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
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public interface IRuleEventRepository
    {
        Task EnqueueAsync(RuleJob job, Instant nextAttempt);

        Task EnqueueAsync(Guid id, Instant nextAttempt);

        Task CancelAsync(Guid id);

        Task MarkSentAsync(RuleJob job, string dump, RuleResult result, RuleJobResult jobResult, TimeSpan elapsed, Instant finished, Instant? nextCall);

        Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken ct = default);

        Task<int> CountByAppAsync(Guid appId);

        Task<IReadOnlyList<RuleStatistics>> QueryStatisticsByAppAsync(Guid appId);

        Task<IReadOnlyList<IRuleEventEntity>> QueryByAppAsync(Guid appId, Guid? ruleId = null, int skip = 0, int take = 20);

        Task<IRuleEventEntity> FindAsync(Guid id);
    }
}
