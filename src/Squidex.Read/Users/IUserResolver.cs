// ==========================================================================
//  IUserResolver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Read.Users
{
    public interface IUserResolver
    {
        Task<IUser> FindById(string id);
    }
}
