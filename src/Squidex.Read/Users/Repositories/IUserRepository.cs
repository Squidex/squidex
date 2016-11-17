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
        Task<List<IUserEntity>> FindUsersByEmail(string email);

        Task<IUserEntity> FindUserByIdAsync(string id);
    }
}
