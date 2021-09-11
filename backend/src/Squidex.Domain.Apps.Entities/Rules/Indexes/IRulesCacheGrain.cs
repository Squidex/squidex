// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public interface IRulesCacheGrain : IGrainWithStringKey
    {
        Task<IReadOnlyCollection<DomainId>> GetRuleIdsAsync();

        Task AddAsync(DomainId id);

        Task RemoveAsync(DomainId id);
    }
}
