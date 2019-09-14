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
    public interface IRulesIndex
    {
        Task<List<IRuleEntity>> GetRulesAsync(Guid appId);

        Task RebuildAsync(Guid appId, HashSet<Guid> rules);
    }
}