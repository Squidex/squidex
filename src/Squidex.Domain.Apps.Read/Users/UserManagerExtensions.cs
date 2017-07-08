// ==========================================================================
//  UserManagerExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable InvertIf
// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace Squidex.Domain.Apps.Read.Users
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

            if (email != null && !string.IsNullOrWhiteSpace(email))
            {
                var upperEmail = email.ToUpperInvariant();

                result = result.Where(x => x.NormalizedEmail.Contains(upperEmail));
            }

            return result;
        }

        public static async Task<IUser> CreateAsync(this UserManager<IUser> userManager, IUserFactory factory, string email, string displayName, string password)
        {
            var user = factory.Create(email);

            user.UpdateDisplayName(displayName);
            user.SetPictureUrlFromGravatar(email);

            await DoChecked(() => userManager.CreateAsync(user), "Cannot create user.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                await DoChecked(() => userManager.AddPasswordAsync(user, password), "Cannot create user.");
            }

            return user;
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
                user.UpdateEmail(email);
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                user.UpdateDisplayName(displayName);
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
