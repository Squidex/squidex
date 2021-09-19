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

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public interface IAppsCacheGrain : IUniqueNameGrain<DomainId>
    {
        Task<IReadOnlyCollection<DomainId>> GetAppIdsAsync(string[] names);

        Task AddAsync(DomainId id, string name);

        Task RemoveAsync(DomainId id);
    }
}
