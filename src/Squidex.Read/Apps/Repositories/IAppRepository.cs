// ==========================================================================
//  IAppRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Read.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<IReadOnlyList<IAppEntity>> QueryAllAsync();

        Task<IAppEntity> FindAppByNameAsync(string name);
    }
}
