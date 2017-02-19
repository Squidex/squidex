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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Infrastructure;
using Squidex.Read.Users;
using Squidex.Read.Users.Repositories;

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

        public async Task LockAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }

        public async Task UnlockAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(id, typeof(IdentityUser));
            }

            await userManager.SetLockoutEndDateAsync(user, null);
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
