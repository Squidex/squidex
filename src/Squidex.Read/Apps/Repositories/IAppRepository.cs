// ==========================================================================
//  IAppRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinkParrot.Read.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<IReadOnlyList<IAppEntity>> QueryAllAsync();

        Task<IAppEntity> FindAppByNameAsync(string name);
    }
}
