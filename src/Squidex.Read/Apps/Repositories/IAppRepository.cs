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

namespace Squidex.Read.Apps.Repositories
{
    public interface IAppRepository
    {
        event Action<Guid> AppSaved;

        Task<IReadOnlyList<IAppEntity>> QueryAllAsync(string subjectId);

        Task<IAppEntity> FindAppAsync(Guid appId);

        Task<IAppEntity> FindAppAsync(string name);
    }
}
