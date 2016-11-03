// ==========================================================================
//  IAppProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Read.Apps.Services
{
    public interface IAppProvider
    {
        Task<IAppEntity> FindAppByNameAsync(string name);
    }
}
