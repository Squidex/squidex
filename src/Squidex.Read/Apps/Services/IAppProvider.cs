// ==========================================================================
//  IAppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Read.Apps.Services
{
    public interface IAppProvider
    {
        Task<IAppEntity> FindAppByIdAsync(Guid id);

        Task<IAppEntity> FindAppByNameAsync(string name);

        void Remove(Guid id);
    }
}
