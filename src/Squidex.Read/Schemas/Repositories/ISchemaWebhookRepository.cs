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
        Task<IReadOnlyList<ISchemaWebhookEntity>> QueryByAppAsync(Guid appId);
    }
}
