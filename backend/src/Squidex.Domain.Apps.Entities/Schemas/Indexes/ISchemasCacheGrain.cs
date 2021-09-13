// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public interface ISchemasCacheGrain : IUniqueNameGrain<DomainId>
    {
        Task<IReadOnlyCollection<DomainId>> GetSchemaIdsAsync();

        Task<DomainId> GetSchemaIdAsync(string name);

        Task AddAsync(DomainId id, string name);

        Task RemoveAsync(DomainId id);
    }
}
