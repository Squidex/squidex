// ==========================================================================
//  IAppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public interface IAppProvider
    {
        Task<IAppEntity> FindAppByIdAsync(Guid id);

        Task<IAppEntity> FindAppByNameAsync(string name);
    }
}
