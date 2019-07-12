// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;

namespace Squidex.Domain.Users
{
    public static class UserManagerExtensions
    {
        public static async Task<UserWithClaims> GetUserWithClaimsAsync(this UserManager<IdentityUser> userManager, ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var user = await userManager.FindByIdWithClaimsAsync(userManager.GetUserId(principal));

            return user;
        }

        public static async Task<UserWithClaims> ResolveUserAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            if (user == null)
            {
                return null;
            }

            var claims = await userManager.GetClaimsAsync(user);

            return new UserWithClaims(user, claims);
        }

        public static async Task<UserWithClaims> FindByIdWithClaimsAsync(this UserManager<IdentityUser> userManager, string id)
        {
            if (id == null)
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(id);

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims> FindByEmailWithClaimsAsyncAsync(this UserManager<IdentityUser> userManager, string email)
        {
            if (email == null)
            {
                return null;
            }

            var user = await userManager.FindByEmailAsync(email);

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims> FindByLoginWithClaimsAsync(this UserManager<IdentityUser> userManager, string loginProvider, string providerKey)
        {
            if (loginProvider == null || providerKey == null)
            {
                return null;
            }

            var user = await userManager.FindByLoginAsync(loginProvider, providerKey);

            return await userManager.ResolveUserAsync(user);
        }

        public static Task<long> CountByEmailAsync(this UserManager<IdentityUser> userManager, string email = null)
        {
            var count = QueryUsers(userManager, email).LongCount();

            return Task.FromResult(count);
        }

        public static async Task<List<UserWithClaims>> QueryByEmailAsync(this UserManager<IdentityUser> userManager, string email = null, int take = 10, int skip = 0)
        {
            var users = QueryUsers(userManager, email).Skip(skip).Take(take).ToList();

            var result = await userManager.ResolveUsersAsync(users);

            return result.ToList();
        }

        public static Task<UserWithClaims[]> ResolveUsersAsync(this UserManager<IdentityUser> userManager, IEnumerable<IdentityUser> users)
        {
            return Task.WhenAll(users.Select(async user =>
            {
                return await userManager.ResolveUserAsync(user);
            }));
        }

        public static IQueryable<IdentityUser> QueryUsers(UserManager<IdentityUser> userManager, string email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = userManager.NormalizeKey(email);

                result = result.Where(x => x.NormalizedEmail.Contains(normalizedEmail));
            }

            return result;
        }

        public static async Task<UserWithClaims> CreateAsync(this UserManager<IdentityUser> userManager, IUserFactory factory, UserValues values)
        {
            var user = factory.Create(values.Email);

            try
            {
                await DoChecked(() => userManager.CreateAsync(user), "Cannot create user.");

                var claims = values.ToClaims().ToList();

                if (claims.Count > 0)
                {
                    await DoChecked(() => userManager.AddClaimsAsync(user, claims), "Cannot add user.");
                }

                if (!string.IsNullOrWhiteSpace(values.Password))
                {
                    await DoChecked(() => userManager.AddPasswordAsync(user, values.Password), "Cannot create user.");
                }
            }
            catch
            {
                await userManager.DeleteAsync(user);

                throw;
            }

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims> UpdateAsync(this UserManager<IdentityUser> userManager, string id, UserValues values)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await UpdateAsync(userManager, user, values);

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<IdentityResult> UpdateSafeAsync(this UserManager<IdentityUser> userManager, IdentityUser user, UserValues values)
        {
            try
            {
                await userManager.UpdateAsync(user, values);

                return IdentityResult.Success;
            }
            catch (ValidationException ex)
            {
                return IdentityResult.Failed(ex.Errors.Select(x => new IdentityError { Description = x.Message }).ToArray());
            }
        }

        public static async Task UpdateAsync(this UserManager<IdentityUser> userManager, IdentityUser user, UserValues values)
        {
            if (user == null)
            {
                throw new DomainObjectNotFoundException("Id", typeof(IdentityUser));
            }

            if (!string.IsNullOrWhiteSpace(values.Email) && values.Email != user.Email)
            {
                await DoChecked(() => userManager.SetEmailAsync(user, values.Email), "Cannot update email.");
                await DoChecked(() => userManager.SetUserNameAsync(user, values.Email), "Cannot update email.");
            }

            await DoChecked(() => userManager.SyncClaimsAsync(user, values.ToClaims().ToList()), "Cannot update user.");

            if (!string.IsNullOrWhiteSpace(values.Password))
            {
                await DoChecked(() => userManager.RemovePasswordAsync(user), "Cannot replace password.");
                await DoChecked(() => userManager.AddPasswordAsync(user, values.Password), "Cannot replace password.");
            }
        }

        public static async Task<UserWithClaims> LockAsync(this UserManager<IdentityUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)), "Cannot lock user.");

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims> UnlockAsync(this UserManager<IdentityUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, null), "Cannot unlock user.");

            return await userManager.ResolveUserAsync(user);
        }

        private static async Task DoChecked(Func<Task<IdentityResult>> action, string message)
        {
            var result = await action();

            if (!result.Succeeded)
            {
                throw new ValidationException(message, result.Errors.Select(x => new ValidationError(x.Description)).ToArray());
            }
        }

        public static async Task<IdentityResult> SyncClaimsAsync(this UserManager<IdentityUser> userManager, IdentityUser user, IEnumerable<Claim> claims)
        {
            if (claims.Any())
            {
                var oldClaims = await userManager.GetClaimsAsync(user);
                var oldClaimsToRemove = new List<Claim>();

                foreach (var oldClaim in oldClaims)
                {
                    if (claims.Any(x => x.Type == oldClaim.Type))
                    {
                        oldClaimsToRemove.Add(oldClaim);
                    }
                }

                if (oldClaimsToRemove.Count > 0)
                {
                    var result = await userManager.RemoveClaimsAsync(user, oldClaimsToRemove);

                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }

                return await userManager.AddClaimsAsync(user, claims);
            }

            return IdentityResult.Success;
        }
    }
}
