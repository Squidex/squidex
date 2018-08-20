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

        Task MarkSentAsync(Guid jobId, string dump, RuleResult result, RuleJobResult jobResult, TimeSpan elapsed, Instant? nextCall);

        Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken ct = default(CancellationToken));

        Task RemoveAsync(Guid appId);

        Task<int> CountByAppAsync(Guid appId);

        Task<IReadOnlyList<IRuleEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20);

        Task<IRuleEventEntity> FindAsync(Guid id);
    }
}
