// ==========================================================================
//  IRuleEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Read.Rules.Repositories
{
    public interface IRuleEventRepository
    {
        Task EnqueueAsync(RuleJob job, Instant nextAttempt);

        Task EnqueueAsync(Guid id, Instant nextAttempt);

        Task MarkSentAsync(Guid jobId, string dump, RuleResult result, RuleJobResult jobResult, TimeSpan elapsed, Instant? nextCall);

        Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> CountByAppAsync(Guid appId);

        Task<IReadOnlyList<IRuleEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20);

        Task<IRuleEventEntity> FindAsync(Guid id);
    }
}
