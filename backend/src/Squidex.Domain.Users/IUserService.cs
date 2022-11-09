// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users;

public interface IUserService
{
    Task<IResultList<IUser>> QueryAsync(IEnumerable<string> ids,
        CancellationToken ct = default);

    Task<IResultList<IUser>> QueryAsync(string? query = null, int take = 10, int skip = 0,
        CancellationToken ct = default);

    string GetUserId(ClaimsPrincipal user,
        CancellationToken ct = default);

    Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user,
        CancellationToken ct = default);

    Task<bool> HasPasswordAsync(IUser user,
        CancellationToken ct = default);

    Task<bool> IsEmptyAsync(
        CancellationToken ct = default);

    Task<IUser> CreateAsync(string email, UserValues? values = null, bool lockAutomatically = false,
        CancellationToken ct = default);

    Task<IUser?> GetAsync(ClaimsPrincipal principal,
        CancellationToken ct = default);

    Task<IUser?> FindByEmailAsync(string email,
        CancellationToken ct = default);

    Task<IUser?> FindByIdAsync(string id,
        CancellationToken ct = default);

    Task<IUser?> FindByLoginAsync(string provider, string key,
        CancellationToken ct = default);

    Task<IUser> SetPasswordAsync(string id, string password, string? oldPassword = null,
        CancellationToken ct = default);

    Task<IUser> AddLoginAsync(string id, ExternalLoginInfo externalLogin,
        CancellationToken ct = default);

    Task<IUser> RemoveLoginAsync(string id, string loginProvider, string providerKey,
        CancellationToken ct = default);

    Task<IUser> LockAsync(string id,
        CancellationToken ct = default);

    Task<IUser> UnlockAsync(string id,
        CancellationToken ct = default);

    Task<IUser> UpdateAsync(string id, UserValues values, bool silent = false,
        CancellationToken ct = default);

    Task DeleteAsync(string id,
        CancellationToken ct = default);
}
