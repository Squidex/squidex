﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users.Implementations
{
    public sealed class DefaultUserService : IUserService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserFactory userFactory;
        private readonly IEnumerable<IUserEvents> userEvents;
        private readonly ISemanticLog log;

        public DefaultUserService(UserManager<IdentityUser> userManager, IUserFactory userFactory,
            IEnumerable<IUserEvents> userEvents, ISemanticLog log)
        {
            Guard.NotNull(userManager, nameof(userManager));
            Guard.NotNull(userFactory, nameof(userFactory));
            Guard.NotNull(userEvents, nameof(userEvents));
            Guard.NotNull(log, nameof(log));

            this.userManager = userManager;
            this.userFactory = userFactory;
            this.userEvents = userEvents;

            this.log = log;
        }

        public async Task<bool> IsEmptyAsync()
        {
            var result = await QueryAsync(null, 0, 0);

            return result.Total == 0;
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            Guard.NotNull(user, nameof(user));

            return userManager.GetUserId(user);
        }

        public async Task<IResultList<IUser>> QueryAsync(IEnumerable<string> ids)
        {
            Guard.NotNull(ids, nameof(ids));

            ids = ids.Where(userFactory.IsId);

            if (!ids.Any())
            {
                return ResultList.CreateFrom<IUser>(0);
            }

            var users = userManager.Users.Where(x => ids.Contains(x.Id)).ToList();

            var resolved = await ResolveAsync(users);

            return ResultList.Create(users.Count, resolved);
        }

        public async Task<IResultList<IUser>> QueryAsync(string? query, int take, int skip)
        {
            IQueryable<IdentityUser> QueryUsers(string? email = null)
            {
                var result = userManager.Users;

                if (!string.IsNullOrWhiteSpace(email))
                {
                    var normalizedEmail = userManager.NormalizeEmail(email);

                    result = result.Where(x => x.NormalizedEmail.Contains(normalizedEmail));
                }

                return result;
            }

            var userItems = QueryUsers(query).Take(take).Skip(skip).ToList();
            var userTotal = QueryUsers(query).LongCount();

            var resolved = await ResolveAsync(userItems);

            return ResultList.Create(userTotal, resolved);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var user = await GetUserAsync(id);

            return await userManager.GetLoginsAsync(user);
        }

        public async Task<bool> GetHasPasswordAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var user = await GetUserAsync(id);

            return await userManager.HasPasswordAsync(user);
        }

        public async Task<IUser?> FindByLoginAsync(string provider, string key)
        {
            Guard.NotNullOrEmpty(provider, nameof(provider));

            var user = await userManager.FindByLoginAsync(provider, key);

            return await ResolveOptionalAsync(user);
        }

        public async Task<IUser?> FindByEmailAsync(string email)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            var user = await userManager.FindByEmailAsync(email);

            return await ResolveOptionalAsync(user);
        }

        public async Task<IUser?> GetAsync(ClaimsPrincipal principal)
        {
            Guard.NotNull(principal, nameof(principal));

            var user = await userManager.GetUserAsync(principal);

            return await ResolveOptionalAsync(user);
        }

        public async Task<IUser?> FindByIdAsync(string id)
        {
            if (!userFactory.IsId(id))
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(id);

            return await ResolveOptionalAsync(user);
        }

        public async Task<IUser> CreateAsync(string email, UserValues? values = null, bool lockAutomatically = false)
        {
            Guard.NotNullOrEmpty(email, nameof(email));

            var user = userFactory.Create(email);

            try
            {
                var isFirst = !userManager.Users.Any();

                await userManager.CreateAsync(user).Throw(log);

                values ??= new UserValues();

                if (isFirst)
                {
                    var permissions = values.Permissions?.ToIds().ToList() ?? new List<string>();

                    permissions.Add(Permissions.Admin);

                    values.Permissions = new PermissionSet(permissions);
                }

                await userManager.SyncClaims(user, values).Throw(log);

                if (!string.IsNullOrWhiteSpace(values.Password))
                {
                    await userManager.AddPasswordAsync(user, values.Password).Throw(log);
                }

                if (!isFirst && lockAutomatically)
                {
                    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                }
            }
            catch (Exception)
            {
                await userManager.DeleteAsync(user);
                throw;
            }

            var resolved = await ResolveAsync(user);

            foreach (var @events in userEvents)
            {
                @events.OnUserRegistered(resolved);
            }

            if (resolved.Claims.HasConsent())
            {
                foreach (var @events in userEvents)
                {
                    @events.OnConsentGiven(resolved);
                }
            }

            return resolved;
        }

        public Task<IUser> SetPasswordAsync(string id, string password, string? oldPassword)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

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

        public async Task<IUser> UpdateAsync(string id, UserValues values)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(values, nameof(values));

            var user = await GetUserAsync(id);

            var oldUser = await ResolveAsync(user);

            var withoutConsent = !oldUser.Claims.HasConsent();

            if (!string.IsNullOrWhiteSpace(values.Email) && values.Email != user.Email)
            {
                await userManager.SetEmailAsync(user, values.Email).Throw(log);
                await userManager.SetUserNameAsync(user, values.Email).Throw(log);
            }

            await userManager.SyncClaims(user, values).Throw(log);

            if (!string.IsNullOrWhiteSpace(values.Password))
            {
                await userManager.RemovePasswordAsync(user).Throw(log);
                await userManager.AddPasswordAsync(user, values.Password).Throw(log);
            }

            var resolved = await ResolveAsync(user);

            foreach (var @events in userEvents)
            {
                @events.OnUserUpdated(resolved);
            }

            if (resolved.Claims.HasConsent() && !withoutConsent)
            {
                foreach (var @events in userEvents)
                {
                    @events.OnConsentGiven(resolved);
                }
            }

            return resolved;
        }

        public Task<IUser> LockAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)).Throw(log));
        }

        public Task<IUser> UnlockAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return ForUserAsync(id, user => userManager.SetLockoutEndDateAsync(user, null).Throw(log));
        }

        public Task<IUser> AddLoginAsync(string id, ExternalLoginInfo externalLogin)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return ForUserAsync(id, user => userManager.AddLoginAsync(user, externalLogin).Throw(log));
        }

        public Task<IUser> RemoveLoginAsync(string id, string loginProvider, string providerKey)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return ForUserAsync(id, user => userManager.RemoveLoginAsync(user, loginProvider, providerKey).Throw(log));
        }

        public async Task DeleteAsync(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var user = await GetUserAsync(id);

            var resolved = await ResolveAsync(user);

            await userManager.DeleteAsync(user);

            foreach (var @events in userEvents)
            {
                @events.OnUserDeleted(resolved);
            }
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

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id);
            }

            return user;
        }

        private Task<IUser[]> ResolveAsync(IEnumerable<IdentityUser> users)
        {
            return Task.WhenAll(users.Select(async user =>
            {
                return await ResolveAsync(user);
            }));
        }

        private async Task<IUser> ResolveAsync(IdentityUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);

            return new UserWithClaims(user, claims.ToList());
        }

        private async Task<IUser?> ResolveOptionalAsync(IdentityUser? user)
        {
            if (user == null)
            {
                return null;
            }

            return await ResolveAsync(user);
        }
    }
}
