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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUserStore :
        MongoRepositoryBase<MongoUser>,
        IUserAuthenticationTokenStore<IdentityUser>,
        IUserAuthenticatorKeyStore<IdentityUser>,
        IUserClaimStore<IdentityUser>,
        IUserEmailStore<IdentityUser>,
        IUserFactory,
        IUserLockoutStore<IdentityUser>,
        IUserLoginStore<IdentityUser>,
        IUserPasswordStore<IdentityUser>,
        IUserPhoneNumberStore<IdentityUser>,
        IUserRoleStore<IdentityUser>,
        IUserSecurityStampStore<IdentityUser>,
        IUserTwoFactorStore<IdentityUser>,
        IUserTwoFactorRecoveryCodeStore<IdentityUser>,
        IQueryableUserStore<IdentityUser>
    {
        private const string InternalLoginProvider = "[AspNetUserStore]";
        private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        private const string RecoveryCodeTokenName = "RecoveryCodes";

        static MongoUserStore()
        {
            BsonClassMap.RegisterClassMap<Claim>(cm =>
            {
                cm.MapConstructor(typeof(Claim).GetConstructors()
                    .First(x =>
                    {
                        var parameters = x.GetParameters();

                        return parameters.Length == 2 &&
                            parameters[0].Name == "type" &&
                            parameters[0].ParameterType == typeof(string) &&
                            parameters[1].Name == "value" &&
                            parameters[1].ParameterType == typeof(string);
                    }))
                    .SetArguments(new[]
                    {
                        nameof(Claim.Type),
                        nameof(Claim.Value)
                    });

                cm.MapMember(x => x.Type);
                cm.MapMember(x => x.Value);
            });

            BsonClassMap.RegisterClassMap<UserLoginInfo>(cm =>
            {
                cm.MapConstructor(typeof(UserLoginInfo).GetConstructors().First())
                    .SetArguments(new[]
                    {
                        nameof(UserLoginInfo.LoginProvider),
                        nameof(UserLoginInfo.ProviderKey),
                        nameof(UserLoginInfo.ProviderDisplayName)
                    });

                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<IdentityUserToken<string>>(cm =>
            {
                cm.AutoMap();

                cm.UnmapMember(x => x.UserId);
            });

            BsonClassMap.RegisterClassMap<IdentityUser<string>>(cm =>
            {
                cm.AutoMap();

                cm.MapMember(x => x.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(x => x.AccessFailedCount).SetIgnoreIfDefault(true);
                cm.MapMember(x => x.EmailConfirmed).SetIgnoreIfDefault(true);
                cm.MapMember(x => x.LockoutEnd).SetElementName("LockoutEndDateUtc").SetIgnoreIfNull(true);
                cm.MapMember(x => x.LockoutEnabled).SetIgnoreIfDefault(true);
                cm.MapMember(x => x.PasswordHash).SetIgnoreIfNull(true);
                cm.MapMember(x => x.PhoneNumber).SetIgnoreIfNull(true);
                cm.MapMember(x => x.PhoneNumberConfirmed).SetIgnoreIfDefault(true);
                cm.MapMember(x => x.SecurityStamp).SetIgnoreIfNull(true);
                cm.MapMember(x => x.TwoFactorEnabled).SetIgnoreIfDefault(true);
            });
        }

        public MongoUserStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_Users";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoUser> collection, CancellationToken ct = default)
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

        public IQueryable<IdentityUser> Users
        {
            get { return Collection.AsQueryable(); }
        }

        public bool IsId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        public IdentityUser Create(string email)
        {
            return new MongoUser { Email = email, UserName = email };
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Id == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedEmail == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Logins.Any(y => y.LoginProvider == loginProvider && y.ProviderKey == providerKey)).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await Collection.Find(x => x.Claims.Any(y => y.Type == claim.Type && y.Value == claim.Value)).ToListAsync(cancellationToken)).OfType<IdentityUser>().ToList();
        }

        public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return (await Collection.Find(x => x.Roles.Contains(roleName)).ToListAsync(cancellationToken)).OfType<IdentityUser>().ToList();
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            user.Id = ObjectId.GenerateNewId().ToString();

            await Collection.InsertOneAsync((MongoUser)user, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            await Collection.ReplaceOneAsync(x => x.Id == user.Id, (MongoUser)user, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            await Collection.DeleteOneAsync(x => x.Id == user.Id, null, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Id);
        }

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).UserName);
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PasswordHash);
        }

        public Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<string>>(((MongoUser)user).Roles.ToList());
        }

        public Task<bool> IsInRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Roles.Contains(roleName));
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<UserLoginInfo>>(((MongoUser)user).Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList());
        }

        public Task<string> GetSecurityStampAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).SecurityStamp);
        }

        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).EmailConfirmed);
        }

        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).NormalizedEmail);
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<Claim>>(((MongoUser)user).Claims);
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).PhoneNumberConfirmed);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).TwoFactorEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).LockoutEnd);
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).LockoutEnabled);
        }

        public Task<string> GetTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).GetToken(loginProvider, name));
        }

        public Task<string> GetAuthenticatorKeyAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).GetToken(InternalLoginProvider, AuthenticatorKeyTokenName));
        }

        public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(((MongoUser)user).PasswordHash));
        }

        public Task<int> CountCodesAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName)?.Split(';').Length ?? 0);
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).UserName = userName;

            return TaskHelper.Done;
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).NormalizedUserName = normalizedName;

            return TaskHelper.Done;
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PasswordHash = passwordHash;

            return TaskHelper.Done;
        }

        public Task AddToRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddRole(roleName);

            return TaskHelper.Done;
        }

        public Task RemoveFromRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveRole(roleName);

            return TaskHelper.Done;
        }

        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddLogin(login);

            return TaskHelper.Done;
        }

        public Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveLogin(loginProvider, providerKey);

            return TaskHelper.Done;
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SecurityStamp = stamp;

            return TaskHelper.Done;
        }

        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            ((MongoUser)user).Email = email;

            return TaskHelper.Done;
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoUser)user).EmailConfirmed = confirmed;

            return TaskHelper.Done;
        }

        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            ((MongoUser)user).NormalizedEmail = normalizedEmail;

            return TaskHelper.Done;
        }

        public Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AddClaims(claims);

            return TaskHelper.Done;
        }

        public Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            ((MongoUser)user).ReplaceClaim(claim, newClaim);

            return TaskHelper.Done;
        }

        public Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveClaims(claims);

            return TaskHelper.Done;
        }

        public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PhoneNumber = phoneNumber;

            return TaskHelper.Done;
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoUser)user).PhoneNumberConfirmed = confirmed;

            return TaskHelper.Done;
        }

        public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoUser)user).TwoFactorEnabled = enabled;

            return TaskHelper.Done;
        }

        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            ((MongoUser)user).LockoutEnd = lockoutEnd?.UtcDateTime;

            return TaskHelper.Done;
        }

        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AccessFailedCount++;

            return Task.FromResult(((MongoUser)user).AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            ((MongoUser)user).AccessFailedCount = 0;

            return TaskHelper.Done;
        }

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoUser)user).LockoutEnabled = enabled;

            return TaskHelper.Done;
        }

        public Task SetTokenAsync(IdentityUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SetToken(loginProvider, name, value);

            return TaskHelper.Done;
        }

        public Task RemoveTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            ((MongoUser)user).RemoveToken(loginProvider, name);

            return TaskHelper.Done;
        }

        public Task SetAuthenticatorKeyAsync(IdentityUser user, string key, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SetToken(InternalLoginProvider, AuthenticatorKeyTokenName, key);

            return TaskHelper.Done;
        }

        public Task ReplaceCodesAsync(IdentityUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            ((MongoUser)user).SetToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", recoveryCodes));

            return TaskHelper.Done;
        }

        public Task<bool> RedeemCodeAsync(IdentityUser user, string code, CancellationToken cancellationToken)
        {
            var mergedCodes = ((MongoUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName) ?? string.Empty;

            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));

                ((MongoUser)user).SetToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", updatedCodes));

                return TaskHelper.True;
            }

            return TaskHelper.False;
        }
    }
}