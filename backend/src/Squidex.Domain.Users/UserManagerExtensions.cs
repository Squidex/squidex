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
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Users
{
    public static class UserManagerExtensions
    {
        public static async Task<UserWithClaims?> GetUserWithClaimsAsync(this UserManager<IdentityUser> userManager, ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var user = await userManager.FindByIdWithClaimsAsync(userManager.GetUserId(principal));

            return user;
        }

        public static async Task<UserWithClaims?> ResolveUserAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            if (user == null)
            {
                return null;
            }

            var claims = await userManager.GetClaimsAsync(user);

            return new UserWithClaims(user, claims);
        }

        public static async Task<UserWithClaims?> FindByIdWithClaimsAsync(this UserManager<IdentityUser> userManager, string id)
        {
            if (id == null)
            {
                return null;
            }

            var user = await userManager.FindByIdAsync(id);

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims?> FindByEmailWithClaimsAsync(this UserManager<IdentityUser> userManager, string email)
        {
            if (email == null)
            {
                return null;
            }

            var user = await userManager.FindByEmailAsync(email);

            return await userManager.ResolveUserAsync(user);
        }

        public static async Task<UserWithClaims?> FindByLoginWithClaimsAsync(this UserManager<IdentityUser> userManager, string loginProvider, string providerKey)
        {
            if (loginProvider == null || providerKey == null)
            {
                return null;
            }

            var user = await userManager.FindByLoginAsync(loginProvider, providerKey);

            return await userManager.ResolveUserAsync(user);
        }

        public static Task<long> CountByEmailAsync(this UserManager<IdentityUser> userManager, string? email = null)
        {
            var count = QueryUsers(userManager, email).LongCount();

            return Task.FromResult(count);
        }

        public static async Task<List<UserWithClaims>> QueryByIdsAync(this UserManager<IdentityUser> userManager, string[] ids)
        {
            var users = userManager.Users.Where(x => ids.Contains(x.Id)).ToList();

            var result = await userManager.ResolveUsersAsync(users);

            return result.ToList();
        }

        public static async Task<List<UserWithClaims>> QueryByEmailAsync(this UserManager<IdentityUser> userManager, string? email = null, int take = 10, int skip = 0)
        {
            var users = QueryUsers(userManager, email).Skip(skip).Take(take).ToList();

            var result = await userManager.ResolveUsersAsync(users);

            return result.ToList();
        }

        public static Task<UserWithClaims[]> ResolveUsersAsync(this UserManager<IdentityUser> userManager, IEnumerable<IdentityUser> users)
        {
            return Task.WhenAll(users.Select(async user =>
            {
                return (await userManager.ResolveUserAsync(user))!;
            }));
        }

        public static IQueryable<IdentityUser> QueryUsers(UserManager<IdentityUser> userManager, string? email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = userManager.NormalizeEmail(email);

                result = result.Where(x => x.NormalizedEmail.Contains(normalizedEmail));
            }

            return result;
        }

        public static async Task<UserWithClaims> CreateAsync(this UserManager<IdentityUser> userManager, IUserFactory factory, UserValues values)
        {
            var user = factory.Create(values.Email);

            try
            {
                await DoChecked(() => userManager.CreateAsync(user));
                await DoChecked(() => values.SyncClaims(userManager, user));

                if (!string.IsNullOrWhiteSpace(values.Password))
                {
                    await DoChecked(() => userManager.AddPasswordAsync(user, values.Password));
                }
            }
            catch
            {
                await userManager.DeleteAsync(user);

                throw;
            }

            return (await userManager.ResolveUserAsync(user))!;
        }

        public static async Task<UserWithClaims> UpdateAsync(this UserManager<IdentityUser> userManager, string id, UserValues values)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id);
            }

            await UpdateAsync(userManager, user, values);

            return (await userManager.ResolveUserAsync(user))!;
        }

        public static Task<IdentityResult> GenerateClientSecretAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var update = new UserValues
            {
                ClientSecret = RandomHash.New()
            };

            return update.SyncClaims(userManager, user);
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
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(values, nameof(values));

            if (!string.IsNullOrWhiteSpace(values.Email) && values.Email != user.Email)
            {
                await DoChecked(() => userManager.SetEmailAsync(user, values.Email));
                await DoChecked(() => userManager.SetUserNameAsync(user, values.Email));
            }

            await DoChecked(() => values.SyncClaims(userManager, user));

            if (!string.IsNullOrWhiteSpace(values.Password))
            {
                await DoChecked(() => userManager.RemovePasswordAsync(user));
                await DoChecked(() => userManager.AddPasswordAsync(user, values.Password));
            }
        }

        public static async Task<UserWithClaims> LockAsync(this UserManager<IdentityUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id);
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)));

            return (await userManager.ResolveUserAsync(user))!;
        }

        public static async Task<UserWithClaims> UnlockAsync(this UserManager<IdentityUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id);
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, null));

            return (await userManager.ResolveUserAsync(user))!;
        }

        private static async Task DoChecked(Func<Task<IdentityResult>> action)
        {
            var result = await action();

            if (!result.Succeeded)
            {
                throw new ValidationException(result.Errors.Select(x => new ValidationError(x.Localize())).ToList());
            }
        }

        public static Task<IdentityResult> SyncClaims(this UserManager<IdentityUser> userManager, IdentityUser user, UserValues values)
        {
            return values.SyncClaims(userManager, user);
        }

        public static string Localize(this IdentityResult result)
        {
            return string.Join(". ", result.Errors.Select(x => x.Localize()));
        }

        public static string Localize(this IdentityError error)
        {
            if (!string.IsNullOrWhiteSpace(error.Code))
            {
                return T.Get($"dotnet_identity_{error.Code}", error.Description);
            }
            else
            {
                return error.Description;
            }
        }
    }
}
