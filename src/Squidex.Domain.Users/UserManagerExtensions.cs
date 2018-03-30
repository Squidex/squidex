// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public static class UserManagerExtensions
    {
        public static Task<IReadOnlyList<IUser>> QueryByEmailAsync(this UserManager<IUser> userManager, string email = null, int take = 10, int skip = 0)
        {
            var users = QueryUsers(userManager, email).Skip(skip).Take(take).ToList();

            return Task.FromResult<IReadOnlyList<IUser>>(users);
        }

        public static Task<long> CountByEmailAsync(this UserManager<IUser> userManager, string email = null)
        {
            var count = QueryUsers(userManager, email).LongCount();

            return Task.FromResult(count);
        }

        private static IQueryable<IUser> QueryUsers(UserManager<IUser> userManager, string email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = userManager.NormalizeKey(email);

                result = result.Where(x => x.NormalizedEmail.Contains(normalizedEmail));
            }

            return result;
        }

        public static async Task<IUser> CreateAsync(this UserManager<IUser> userManager, IUserFactory factory, string email, string displayName, string password)
        {
            var user = factory.Create(email);

            try
            {
                user.SetDisplayName(displayName);
                user.SetPictureUrlFromGravatar(email);

                await DoChecked(() => userManager.CreateAsync(user), "Cannot create user.");

                if (!string.IsNullOrWhiteSpace(password))
                {
                    await DoChecked(() => userManager.AddPasswordAsync(user, password), "Cannot create user.");
                }
            }
            catch
            {
                await userManager.DeleteAsync(user);

                throw;
            }

            return user;
        }

        public static Task<IdentityResult> UpdateAsync(this UserManager<IUser> userManager, IUser user, string email, string displayName, bool hidden)
        {
            user.SetHidden(hidden);
            user.SetEmail(email);
            user.SetDisplayName(displayName);

            return userManager.UpdateAsync(user);
        }

        public static async Task UpdateAsync(this UserManager<IUser> userManager, string id, string email, string displayName, string password)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IUser));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                await DoChecked(() => userManager.SetEmailAsync(user, email), "Cannot update email.");
                await DoChecked(() => userManager.SetUserNameAsync(user, email), "Cannot update email.");
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                user.SetDisplayName(displayName);
            }

            await DoChecked(() => userManager.UpdateAsync(user), "Cannot update user.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                await DoChecked(() => userManager.RemovePasswordAsync(user), "Cannot update user.");
                await DoChecked(() => userManager.AddPasswordAsync(user, password), "Cannot update user.");
            }
        }

        public static async Task LockAsync(this UserManager<IUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IUser));
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)), "Cannot lock user.");
        }

        public static async Task UnlockAsync(this UserManager<IUser> userManager, string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IUser));
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, null), "Cannot unlock user.");
        }

        private static async Task DoChecked(Func<Task<IdentityResult>> action, string message)
        {
            var result = await action();

            if (!result.Succeeded)
            {
                throw new ValidationException(message, result.Errors.Select(x => new ValidationError(x.Description)).ToArray());
            }
        }
    }
}
