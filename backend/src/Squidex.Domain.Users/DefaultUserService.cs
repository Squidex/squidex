﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users;

public sealed class DefaultUserService(
    UserManager<IdentityUser> userManager,
    IUserFactory userFactory,
    IEnumerable<IUserEvents> userEvents,
    ILogger<DefaultUserService> log)
    : IUserService
{
    public async Task<bool> IsEmptyAsync(
        CancellationToken ct = default)
    {
        var result = await QueryAsync(null, 1, 0, ct);

        return result.Total == 0;
    }

    public string GetUserId(ClaimsPrincipal user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.GetUserId(user)!;
    }

    public async Task<IResultList<IUser>> QueryAsync(IEnumerable<string> ids,
        CancellationToken ct = default)
    {
        Guard.NotNull(ids);

        ids = ids.Where(userFactory.IsId);
        if (!ids.Any())
        {
            return ResultList.Empty<IUser>();
        }

        var userItems = userManager.Users.Where(x => ids.Contains(x.Id)).ToList();
        var userTotal = userItems.Count;
        var resolved = await ResolveAsync(userItems);

        return ResultList.Create(userTotal, resolved);
    }

    public async Task<IResultList<IUser>> QueryAsync(string? query = null, int take = 10, int skip = 0,
        CancellationToken ct = default)
    {
        Guard.GreaterThan(take, 0);
        Guard.GreaterEquals(skip, 0);

        IQueryable<IdentityUser> QueryUsers(string? email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = userManager.NormalizeEmail(email);

                result = result.Where(x => x.NormalizedEmail!.Contains(normalizedEmail));
            }

            return result;
        }

        var userItems = QueryUsers(query).OrderBy(x => x.NormalizedEmail).Skip(skip).Take(take).ToList();
        var userTotal = QueryUsers(query).LongCount();
        var resolved = await ResolveAsync(userItems);

        return ResultList.Create(userTotal, resolved);
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.GetLoginsAsync((IdentityUser)user.Identity);
    }

    public Task<bool> HasPasswordAsync(IUser user,
        CancellationToken ct = default)
    {
        Guard.NotNull(user);

        return userManager.HasPasswordAsync((IdentityUser)user.Identity);
    }

    public async Task<IUser?> FindByLoginAsync(string provider, string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(provider);

        var user = await userManager.FindByLoginAsync(provider, key);

        return user != null ? await ResolveAsync(user) : null;
    }

    public async Task<IUser?> FindByEmailAsync(string email,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(email);

        var user = await userManager.FindByEmailAsync(email);

        return user != null ? await ResolveAsync(user) : null;
    }

    public async Task<IUser?> GetAsync(ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        Guard.NotNull(principal);

        var user = await userManager.GetUserAsync(principal);

        return user != null ? await ResolveAsync(user) : null;
    }

    public async Task<IUser?> FindByIdAsync(string id,
        CancellationToken ct = default)
    {
        if (!userFactory.IsId(id))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(id);

        return user != null ? await ResolveAsync(user) : null;
    }

    public async Task<IUser> CreateAsync(string email, UserValues? values = null, bool lockAutomatically = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(email);

        var isFirst = !userManager.Users.Any();

        var user = userFactory.Create(email);

        try
        {
            await userManager.CreateAsync(user).Throw(log);

            values ??= new UserValues();

            if (string.IsNullOrWhiteSpace(values.DisplayName))
            {
                values.DisplayName = email;
            }

            if (isFirst)
            {
                var permissions = values.Permissions?.ToIds().ToList() ?? [];

                permissions.Add(PermissionIds.Admin);

                values.Permissions = new PermissionSet(permissions);
            }

            await userManager.SyncClaims(user, values).Throw(log);

            if (!string.IsNullOrWhiteSpace(values.Password))
            {
                await userManager.AddPasswordAsync(user, values.Password).Throw(log);
            }

            if (!isFirst && lockAutomatically)
            {
                await userManager.SetLockoutEndDateAsync(user, LockoutDate()).Throw(log);
            }
        }
        catch (Exception)
        {
            try
            {
                if (userFactory.IsId(user.Id))
                {
                    await userManager.DeleteAsync(user);
                }
            }
            catch (Exception ex2)
            {
                log.LogError(ex2, "Failed to cleanup user after creation failed.");
            }

            throw;
        }

        var resolved = await ResolveAsync(user);

        foreach (var events in userEvents)
        {
            await events.OnUserRegisteredAsync(resolved);
        }

        if (HasConsentGiven(values, null!))
        {
            foreach (var events in userEvents)
            {
                await events.OnConsentGivenAsync(resolved);
            }
        }

        return resolved;
    }

    public Task<IUser> SetPasswordAsync(string id, string password, string? oldPassword = null,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, async user =>
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await userManager.ChangePasswordAsync(user, oldPassword!, password).Throw(log);
            }
            else
            {
                await userManager.AddPasswordAsync(user, password).Throw(log);
            }
        });
    }

    public async Task<IUser> UpdateAsync(string id, UserValues values, bool silent = false,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(values);

        var user = await GetUserAsync(id);

        var oldUser = await ResolveAsync(user);

        if (!string.IsNullOrWhiteSpace(values.Email) && values.Email != user.Email)
        {
            await userManager.SetEmailAsync(user, values.Email).Throw(log);
            await userManager.SetUserNameAsync(user, values.Email).Throw(log);
        }

        await userManager.SyncClaims(user, values).Throw(log);

        if (!string.IsNullOrWhiteSpace(values.Password))
        {
            if (await userManager.HasPasswordAsync(user))
            {
                await userManager.RemovePasswordAsync(user).Throw(log);
            }

            await userManager.AddPasswordAsync(user, values.Password).Throw(log);
        }

        var resolved = await ResolveAsync(user);

        if (!silent)
        {
            foreach (var events in userEvents)
            {
                await events.OnUserUpdatedAsync(resolved, oldUser);
            }

            if (HasConsentGiven(values, oldUser))
            {
                foreach (var events in userEvents)
                {
                    await events.OnConsentGivenAsync(resolved);
                }
            }
        }

        return resolved;
    }

    public Task<IUser> LockAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, LockoutDate()).Throw(log));
    }

    public Task<IUser> UnlockAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, null).Throw(log));
    }

    public Task<IUser> AddLoginAsync(string id, ExternalLoginInfo externalLogin,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.AddLoginAsync(user, externalLogin).Throw(log));
    }

    public Task<IUser> RemoveLoginAsync(string id, string loginProvider, string providerKey,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        return ForUserAsync(id, user => userManager.RemoveLoginAsync(user, loginProvider, providerKey).Throw(log));
    }

    public async Task DeleteAsync(string id,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);

        var user = await GetUserAsync(id);

        if (userEvents.Any())
        {
            var resolved = await ResolveAsync(user);

            foreach (var events in userEvents)
            {
                await events.OnUserDeletedAsync(resolved);
            }
        }

        await userManager.DeleteAsync(user).Throw(log);
    }

    private async Task<IUser> ForUserAsync(string id, Func<IdentityUser, Task> action)
    {
        var user = await GetUserAsync(id);

        await action(user);

        return await ResolveAsync(user);
    }

    private async Task<IdentityUser> GetUserAsync(string id)
    {
        if (!userFactory.IsId(id))
        {
            throw new DomainObjectNotFoundException(id);
        }

        var user = await userManager.FindByIdAsync(id);

        return user ?? throw new DomainObjectNotFoundException(id);
    }

    private async Task<List<IUser>> ResolveAsync(IEnumerable<IdentityUser> users)
    {
        var result = new List<IUser>();
        foreach (var user in users)
        {
            result.Add(await ResolveAsync(user));
        }

        return result;
    }

    private async Task<IUser> ResolveAsync(IdentityUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);

        if (!claims.Any(x => string.Equals(x.Type, SquidexClaimTypes.DisplayName, StringComparison.OrdinalIgnoreCase)))
        {
            claims.Add(new Claim(SquidexClaimTypes.DisplayName, user.Email!));
        }

        return new UserWithClaims(user, claims.ToList());
    }

    private static bool HasConsentGiven(UserValues values, IUser? oldUser)
    {
        if (values.Consent == true && oldUser?.Claims.HasConsent() != true)
        {
            return true;
        }

        return values.ConsentForEmails == true && oldUser?.Claims.HasConsentForEmails() != true;
    }

    private static DateTimeOffset LockoutDate()
    {
        return DateTimeOffset.UtcNow.AddYears(100);
    }
}
