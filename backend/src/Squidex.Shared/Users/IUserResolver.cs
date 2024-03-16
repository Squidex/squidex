// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Shared.Users
{
    public interface IUserResolver
    {
        Task<(IUser? User, bool Created)> CreateUserIfNotExistsAsync(string email, bool invited = false,
            CancellationToken ct = default);

        Task SetClaimAsync(string id, string type, string value, bool silent = false,
            CancellationToken ct = default);

        Task<IUser?> FindByIdOrEmailAsync(string idOrEmail,
            CancellationToken ct = default);

        Task<IUser?> FindByIdAsync(string idOrEmail,
            CancellationToken ct = default);

        Task<List<IUser>> QueryByEmailAsync(string email,
            CancellationToken ct = default);

        Task<List<IUser>> QueryAllAsync(
            CancellationToken ct = default);

        Task<Dictionary<string, IUser>> QueryManyAsync(string[] ids,
            CancellationToken ct = default);
    }
}
