// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Repositories
{
    public interface ISchemaRepository
    {
        Task<Dictionary<string, DomainId>> QueryIdsAsync(DomainId appId,
            CancellationToken ct = default);
    }
}
