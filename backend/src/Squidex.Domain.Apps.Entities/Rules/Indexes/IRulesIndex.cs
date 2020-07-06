// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public interface IRulesIndex
    {
        Task<List<IRuleEntity>> GetRulesAsync(DomainId appId);

        Task RebuildAsync(DomainId appId, HashSet<DomainId> rules);
    }
}