// ==========================================================================
//  IAppRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace PinkParrot.Read.Apps.Repositories
{
    public interface IAppRepository
    {
        Task<IAppEntity> FindAppByNameAsync(string name);
    }
}
