// ==========================================================================
//  MongoUserRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Core.Identity;
using Squidex.Infrastructure;
using Squidex.Read.Users;
using Squidex.Read.Users.Repositories;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable InvertIf

namespace Squidex.Read.MongoDb.Users
{
    public sealed class MongoUserRepository : IUserRepository
    {
        private readonly UserManager<IdentityUser> userManager;

        public MongoUserRepository(UserManager<IdentityUser> userManager)
        {
            Guard.NotNull(userManager, nameof(userManager));

            this.userManager = userManager;
        }

        public Task<IReadOnlyList<IUserEntity>> QueryByEmailAsync(string email, int take = 10, int skip = 0)
        {
            var users = QueryUsers(email).Skip(skip).Take(take).ToList();
            
            return Task.FromResult<IReadOnlyList<IUserEntity>>(users.Select(x => (IUserEntity)new MongoUserEntity(x)).ToList());
        }

        public Task<long> CountAsync(string email = null)
        {
            var count = QueryUsers(email).LongCount();

            return Task.FromResult(count);
        }

        public async Task<IUserEntity> FindUserByIdAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            return user != null ? new MongoUserEntity(user) : null;
        }

        public async Task<string> CreateAsync(string email, string displayName, string password)
        {
            var pictureUrl = GravatarHelper.CreatePictureUrl(email);

            var user = new IdentityUser
            {
                Email = email,
                Claims = new List<IdentityUserClaim>
                {
                    new IdentityUserClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl)),
                    new IdentityUserClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, displayName))
                },
                UserName = email
            };

            await DoChecked(() => userManager.CreateAsync(user), "Cannot create user.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                await DoChecked(() => userManager.AddPasswordAsync(user, password), "Cannot create user.");
            }

            return user.Id;
        }

        public async Task UpdateAsync(string id, string email, string displayName, string password)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                user.Email = user.UserName = email;
            }

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                user.Claims.Find(x => x.Type == SquidexClaimTypes.SquidexDisplayName).Value = displayName;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.PasswordHash = null;
            }

            await DoChecked(() => userManager.UpdateAsync(user), "Cannot update user.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                await DoChecked(() => userManager.AddPasswordAsync(user, password), "Cannot update user.");
            }
        }

        public async Task LockAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await DoChecked(() => userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)), "Cannot lock user.");
        }

        public async Task UnlockAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
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

        private IQueryable<IdentityUser> QueryUsers(string email = null)
        {
            var result = userManager.Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var upperEmail = email.ToUpperInvariant();

                result = userManager.Users.Where(x => x.NormalizedEmail.Contains(upperEmail));
            }

            return result;
        }
    }
}
