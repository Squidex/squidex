// ==========================================================================
//  IAppRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<Guid> FindAppIdByNameAsync(string name);

        Task<IReadOnlyList<Guid>> QueryUserAppIdsAsync(string userId);
    }
}
