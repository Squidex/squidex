// ==========================================================================
//  IRuleRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Rules.Repositories
{
    public interface IRuleRepository
    {
        Task<IReadOnlyList<IRuleEntity>> QueryByAppAsync(Guid appId);

        Task<IReadOnlyList<IRuleEntity>> QueryCachedByAppAsync(Guid appId);
    }
}
