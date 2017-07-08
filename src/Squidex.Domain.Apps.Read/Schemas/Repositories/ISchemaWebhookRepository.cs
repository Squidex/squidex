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

namespace Squidex.Read.Schemas.Repositories
{
    public interface ISchemaWebhookRepository
    {
        Task AddInvokationAsync(Guid webhookId, string dump, WebhookResult result, TimeSpan elapsed);

        Task<IReadOnlyList<ISchemaWebhookUrlEntity>> QueryUrlsBySchemaAsync(Guid appId, Guid schemaId);

        Task<IReadOnlyList<ISchemaWebhookEntity>> QueryByAppAsync(Guid appId);
    }
}
