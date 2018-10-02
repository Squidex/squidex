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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUserStore :
        MongoRepositoryBase<MongoUser>,
        IUserPasswordStore<IUser>,
        IUserRoleStore<IUser>,
        IUserLoginStore<IUser>,
        IUserSecurityStampStore<IUser>,
        IUserEmailStore<IUser>,
        IUserClaimStore<IUser>,
        IUserPhoneNumberStore<IUser>,
        IUserTwoFactorStore<IUser>,
        IUserLockoutStore<IUser>,
        IUserAuthenticationTokenStore<IUser>,
        IUserFactory,
        IUserResolver,
        IQueryableUserStore<IUser>
    {
        public MongoUserStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_Users";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoUser> collection, CancellationToken ct = default(CancellationToken))
        {
            return collection.Indexes.CreateManyAsync(
                new[]
                {
                    new CreateIndexModel<MongoUser>(Index.Ascending("Logins.LoginProvider").Ascending("Logins.ProviderKey")),
                    new CreateIndexModel<MongoUser>(Index.Ascending(x => x.NormalizedUserName), new CreateIndexOptions { Unique = true }),
                    new CreateIndexModel<MongoUser>(Index.Ascending(x => x.NormalizedEmail), new CreateIndexOptions { Unique = true })
                }, ct);
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public void Dispose()
        {
        }

        public IQueryable<IUser> Users
        {
            get { return Collection.AsQueryable(); }
        }

        public IUser Create(string email)
        {
            return new MongoUser { Email = email, UserName = email };
        }

        public async Task<IUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Id == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedEmail == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Logins.Any(y => y.LoginProvider == loginProvider && y.ProviderKey == providerKey)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<IUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await Collection.Find(x => x.Claims.Any(y => y.Type == claim.Type && y.Value == claim.Value)).ToListAsync(cancellationToken)).OfType<IUser>().ToList();
        }

        public async Task<IList<IUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return (await Collection.Find(x => x.Roles.Contains(roleName)).ToListAsync(cancellationToken)).OfType<IUser>().ToList();
        }

        public async Task<IdentityResult> CreateAsync(IUser user, CancellationToken cancellationToken)
        {
            await Collection.InsertOneAsync((MongoUser)user, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IUser user, CancellationToken cancellationToken)
        {
            await Collection.ReplaceOneAsync(x => x.Id == user.Id, (MongoUser)user, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IUser user, CancellationToken cancellationToken)
        {
            await Collection.DeleteOneAsync(x => x.Id == user.Id, null, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<string> GetUserIdAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Id);
        }

        public Task<string> GetUserNameAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).UserName);
        }

        public Task<string> GetNormalizedUserNameAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PasswordHash);
        }

        public Task<IList<string>> GetRolesAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<string>>(((MongoUser)user).Roles);
        }

        public Task<bool> IsInRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Roles.Contains(roleName));
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<UserLoginInfo>>(((MongoUser)user).Logins.Select(x => (UserLoginInfo)x).ToList());
        }

        public Task<string> GetSecurityStampAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).SecurityStamp);
        }

        public Task<string> GetEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).EmailConfirmed);
        }

        public Task<string> GetNormalizedEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).NormalizedEmail);
        }

        public Task<IList<Claim>> GetClaimsAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<Claim>>(((MongoUser)user).Claims.Select(x => (Claim)x).ToList());
        }

        public Task<string> GetPhoneNumberAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PhoneNumberConfirmed);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).TwoFactorEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<DateTimeOffset?>(((MongoUser)user).LockoutEndDateUtc);
        }

        public Task<int> GetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).LockoutEnabled);
        }

        public Task<string> GetTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).GetToken(loginProvider, name));
        }

        public Task<bool> HasPasswordAsync(IUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(((MongoUser)user).PasswordHash));
        }

        public Task SetUserNameAsync(IUser user, string userName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).UserName = userName;

            return TaskHelper.Done;
        }

        public Task SetNormalizedUserNameAsync(IUser user, string normalizedName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).NormalizedUserName = normalizedName;

            return TaskHelper.Done;
        }

        public Task SetPasswordHashAsync(IUser user, string passwordHash, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PasswordHash = passwordHash;

            return TaskHelper.Done;
        }

        public Task AddToRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddRole(roleName);

            return TaskHelper.Done;
        }

        public Task RemoveFromRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveRole(roleName);

            return TaskHelper.Done;
        }

        public Task AddLoginAsync(IUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddLogin(login);

            return TaskHelper.Done;
        }

        public Task RemoveLoginAsync(IUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveLogin(loginProvider, providerKey);

            return TaskHelper.Done;
        }

        public Task SetSecurityStampAsync(IUser user, string stamp, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SecurityStamp = stamp;

            return TaskHelper.Done;
        }

        public Task SetEmailAsync(IUser user, string email, CancellationToken cancellationToken)
        {
            ((MongoUser)user).Email = email;

            return TaskHelper.Done;
        }

        public Task SetEmailConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoUser)user).EmailConfirmed = confirmed;

            return TaskHelper.Done;
        }

        public Task SetNormalizedEmailAsync(IUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            ((MongoUser)user).NormalizedEmail = normalizedEmail;

            return TaskHelper.Done;
        }

        public Task AddClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddClaims(claims);

            return TaskHelper.Done;
        }

        public Task ReplaceClaimAsync(IUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            ((MongoUser)user).ReplaceClaim(claim, newClaim);

            return TaskHelper.Done;
        }

        public Task RemoveClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveClaims(claims);

            return TaskHelper.Done;
        }

        public Task SetPhoneNumberAsync(IUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PhoneNumber = phoneNumber;

            return TaskHelper.Done;
        }

        public Task SetPhoneNumberConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PhoneNumberConfirmed = confirmed;

            return TaskHelper.Done;
        }

        public Task SetTwoFactorEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoUser)user).TwoFactorEnabled = enabled;

            return TaskHelper.Done;
        }

        public Task SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            ((MongoUser)user).LockoutEndDateUtc = lockoutEnd?.UtcDateTime;

            return TaskHelper.Done;
        }

        public Task<int> IncrementAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AccessFailedCount++;

            return Task.FromResult(((MongoUser)user).AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AccessFailedCount = 0;

            return TaskHelper.Done;
        }

        public Task SetLockoutEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoUser)user).LockoutEnabled = enabled;

            return TaskHelper.Done;
        }

        public Task SetTokenAsync(IUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SetToken(loginProvider, name, value);

            return TaskHelper.Done;
        }

        public Task RemoveTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveToken(loginProvider, name);

            return TaskHelper.Done;
        }

        public async Task<IUser> FindByIdOrEmailAsync(string id)
        {
            if (ObjectId.TryParse(id, out _))
            {
                return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            else
            {
                return await Collection.Find(x => x.NormalizedEmail == id.ToUpperInvariant()).FirstOrDefaultAsync();
            }
        }

        public Task<List<IUser>> QueryByEmailAsync(string email)
        {
            var result = Users;

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.ToUpperInvariant();

                result = result.Where(x => x.NormalizedEmail.Contains(normalizedEmail));
            }

            return Task.FromResult(result.Select(x => x).ToList());
        }
    }
}
