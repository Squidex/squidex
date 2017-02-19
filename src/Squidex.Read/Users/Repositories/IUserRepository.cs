// ==========================================================================
//  IUserRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Read.Users.Repositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<IUserEntity>> QueryByEmailAsync(string email = null, int take = 10, int skip = 0);

        Task<IUserEntity> FindUserByIdAsync(string id);

        Task LockAsync(string id);

        Task UnlockAsync(string id);

        Task<long> CountAsync(string email = null);
    }
}
