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

namespace Squidex.Domain.Apps.Read.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<IReadOnlyList<IAppEntity>> QueryAllAsync(string subjectId);

        Task<IAppEntity> FindAppAsync(Guid appId);

        Task<IAppEntity> FindAppAsync(string name);
    }
}
