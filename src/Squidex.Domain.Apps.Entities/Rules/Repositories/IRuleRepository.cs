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

namespace Squidex.Domain.Apps.Entities.Rules.Repositories
{
    public interface IRuleRepository
    {
        Task<IReadOnlyList<string>> QueryRuleIdsAsync(Guid appId);
    }
}
