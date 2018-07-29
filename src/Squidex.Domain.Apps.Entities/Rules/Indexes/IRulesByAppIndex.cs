// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public interface IRulesByAppIndex : ICleanableAppGrain
    {
        Task AddRuleAsync(Guid ruleId);

        Task RemoveRuleAsync(Guid ruleId);

        Task RebuildAsync(HashSet<Guid> rules);

        Task<List<Guid>> GetRuleIdsAsync();
    }
}
