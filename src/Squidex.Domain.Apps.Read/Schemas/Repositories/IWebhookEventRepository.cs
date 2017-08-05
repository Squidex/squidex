// ==========================================================================
//  IWebhookEventRepository.cs
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

namespace Squidex.Domain.Apps.Read.Schemas.Repositories
{
    public interface IWebhookEventRepository
    {
        Task EnqueueAsync(WebhookJob job, Instant nextAttempt);

        Task EnqueueAsync(Guid id, Instant nextAttempt);

        Task TraceSendingAsync(Guid jobId);

        Task TraceSentAsync(Guid jobId, string dump, WebhookResult result, TimeSpan elapsed, Instant? nextCall);

        Task QueryPendingAsync(Func<IWebhookEventEntity, Task> callback, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> CountByAppAsync(Guid appId);

        Task<IReadOnlyList<IWebhookEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20);

        Task<IWebhookEventEntity> FindAsync(Guid id);
    }
}
