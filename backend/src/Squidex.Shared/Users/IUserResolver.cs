// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Shared.Users
{
    public interface IUserResolver
    {
        Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited = false);

        Task<IUser?> FindByIdOrEmailAsync(string idOrEmail);

        Task<IUser?> FindByIdAsync(string idOrEmail);

        Task SetClaimAsync(string id, string type, string value, bool silent = false);

        Task<List<IUser>> QueryByEmailAsync(string email);

        Task<List<IUser>> QueryAllAsync();

        Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids);
    }
}
