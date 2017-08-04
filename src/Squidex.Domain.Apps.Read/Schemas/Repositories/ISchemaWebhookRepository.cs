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

namespace Squidex.Domain.Apps.Read.Schemas.Repositories
{
    public interface ISchemaWebhookRepository
    {
        Task TraceSentAsync(Guid webhookId, WebhookResult result, TimeSpan elapsed);

        Task<IReadOnlyList<ISchemaWebhookUrlEntity>> QueryUrlsBySchemaAsync(Guid appId, Guid schemaId);

        Task<IReadOnlyList<ISchemaWebhookEntity>> QueryByAppAsync(Guid appId);
    }
}
