// ==========================================================================
//  ISchemaWebhookRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Webhooks.Repositories
{
    public interface IWebhookRepository
    {
        Task TraceSentAsync(Guid webhookId, WebhookResult result, TimeSpan elapsed);

        Task<IReadOnlyList<IWebhookEntity>> QueryByAppAsync(Guid appId);
    }
}
