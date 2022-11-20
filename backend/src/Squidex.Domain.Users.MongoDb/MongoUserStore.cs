// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Users.MongoDb;

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

        BsonClassMap.RegisterClassMap<UserLogin>(cm =>
        {
            cm.MapConstructor(typeof(UserLogin).GetConstructors()
                .First(x =>
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 3;
                }))
                .SetArguments(new[]
                {
                    nameof(UserLogin.LoginProvider),
                    nameof(UserLogin.ProviderKey),
                    nameof(UserLogin.ProviderDisplayName)
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

            cm.MapMember(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));

            cm.MapMember(x => x.AccessFailedCount)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.EmailConfirmed)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.LockoutEnd)
                .SetElementName("LockoutEndDateUtc").SetIgnoreIfNull(true);

            cm.MapMember(x => x.LockoutEnabled)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.PasswordHash)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.PhoneNumber)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.PhoneNumberConfirmed)
                .SetIgnoreIfDefault(true);

            cm.MapMember(x => x.SecurityStamp)
                .SetIgnoreIfNull(true);

            cm.MapMember(x => x.TwoFactorEnabled)
                .SetIgnoreIfDefault(true);
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

    protected override Task SetupCollectionAsync(IMongoCollection<MongoUser> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoUser>(
                Index
                    .Ascending("Logins.LoginProvider")
                    .Ascending("Logins.ProviderKey")),
            new CreateIndexModel<MongoUser>(
                Index
                    .Ascending(x => x.NormalizedUserName),
                new CreateIndexOptions
                {
                    Unique = true
                }),
            new CreateIndexModel<MongoUser>(
                Index
                    .Ascending(x => x.NormalizedEmail),
                new CreateIndexOptions
                {
                    Unique = true
                })
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
        get => Collection.AsQueryable();
    }

    public bool IsId(string id)
    {
        return ObjectId.TryParse(id, out _);
    }

    public IdentityUser Create(string email)
    {
        return new MongoUser { Email = email, UserName = email };
    }

    public async Task<IdentityUser?> FindByIdAsync(string userId,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.Id == userId).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IdentityUser?> FindByEmailAsync(string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IdentityUser?> FindByNameAsync(string normalizedUserName,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.NormalizedEmail == normalizedUserName).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IdentityUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.Logins.Any(y => y.LoginProvider == loginProvider && y.ProviderKey == providerKey)).FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.Claims.Any(y => y.Type == claim.Type && y.Value == claim.Value)).ToListAsync(cancellationToken);

        return result.OfType<IdentityUser>().ToList();
    }

    public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName,
        CancellationToken cancellationToken)
    {
        var result = await Collection.Find(x => x.Roles.Contains(roleName)).ToListAsync(cancellationToken);

        return result.OfType<IdentityUser>().ToList();
    }

    public async Task<IdentityResult> CreateAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        user.Id = ObjectId.GenerateNewId().ToString();

        await Collection.InsertOneAsync((MongoUser)user, null, cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        await Collection.ReplaceOneAsync(x => x.Id == user.Id, (MongoUser)user, cancellationToken: cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        await Collection.DeleteOneAsync(x => x.Id == user.Id, null, cancellationToken);

        return IdentityResult.Success;
    }

    public Task<string> GetUserIdAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.Id;

        return Task.FromResult(result);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.UserName;

        return Task.FromResult(result);
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.NormalizedUserName;

        return Task.FromResult(result);
    }

    public Task<string?> GetPasswordHashAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.PasswordHash;

        return Task.FromResult(result);
    }

    public Task<IList<string>> GetRolesAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).Roles.ToList();

        return Task.FromResult<IList<string>>(result);
    }

    public Task<bool> IsInRoleAsync(IdentityUser user, string roleName,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).Roles.Contains(roleName);

        return Task.FromResult(result);
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList();

        return Task.FromResult<IList<UserLoginInfo>>(result);
    }

    public Task<string?> GetSecurityStampAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.SecurityStamp;

        return Task.FromResult(result);
    }

    public Task<string?> GetEmailAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.Email;

        return Task.FromResult(result);
    }

    public Task<bool> GetEmailConfirmedAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.EmailConfirmed;

        return Task.FromResult(result);
    }

    public Task<string?> GetNormalizedEmailAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.NormalizedEmail;

        return Task.FromResult(result);
    }

    public Task<IList<Claim>> GetClaimsAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).Claims;

        return Task.FromResult<IList<Claim>>(result);
    }

    public Task<string?> GetPhoneNumberAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.PhoneNumber;

        return Task.FromResult(result);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.PhoneNumberConfirmed;

        return Task.FromResult(result);
    }

    public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.TwoFactorEnabled;

        return Task.FromResult(result);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.LockoutEnd;

        return Task.FromResult(result);
    }

    public Task<int> GetAccessFailedCountAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.AccessFailedCount;

        return Task.FromResult(result);
    }

    public Task<bool> GetLockoutEnabledAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = user.LockoutEnabled;

        return Task.FromResult(result);
    }

    public Task<string?> GetTokenAsync(IdentityUser user, string loginProvider, string name,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).GetToken(loginProvider, name)!;

        return Task.FromResult<string?>(result);
    }

    public Task<string?> GetAuthenticatorKeyAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).GetToken(InternalLoginProvider, AuthenticatorKeyTokenName)!;

        return Task.FromResult<string?>(result);
    }

    public Task<bool> HasPasswordAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = !string.IsNullOrWhiteSpace(user.PasswordHash);

        return Task.FromResult(result);
    }

    public Task<int> CountCodesAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        var result = ((MongoUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName)?.Split(';').Length ?? 0;

        return Task.FromResult(result);
    }

    public Task SetUserNameAsync(IdentityUser user, string? userName,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).UserName = userName;

        return Task.CompletedTask;
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    public Task AddToRoleAsync(IdentityUser user, string roleName,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).AddRole(roleName);

        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(IdentityUser user, string roleName,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).RemoveRole(roleName);

        return Task.CompletedTask;
    }

    public Task AddLoginAsync(IdentityUser user, UserLoginInfo login,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).AddLogin(login);

        return Task.CompletedTask;
    }

    public Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).RemoveLogin(loginProvider, providerKey);

        return Task.CompletedTask;
    }

    public Task SetSecurityStampAsync(IdentityUser user, string stamp,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    public Task SetEmailAsync(IdentityUser user, string? email,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).Email = email;

        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(IdentityUser user, string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    public Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).AddClaims(claims);

        return Task.CompletedTask;
    }

    public Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).ReplaceClaim(claim, newClaim);

        return Task.CompletedTask;
    }

    public Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).RemoveClaims(claims);

        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(IdentityUser user, string? phoneNumber,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).PhoneNumber = phoneNumber;

        return Task.CompletedTask;
    }

    public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).PhoneNumberConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).TwoFactorEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).LockoutEnd = lockoutEnd?.UtcDateTime;

        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).AccessFailedCount++;

        return Task.FromResult(((MongoUser)user).AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(IdentityUser user,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).AccessFailedCount = 0;

        return Task.CompletedTask;
    }

    public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).LockoutEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task SetTokenAsync(IdentityUser user, string loginProvider, string name, string? value,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).ReplaceToken(loginProvider, name, value);

        return Task.CompletedTask;
    }

    public Task RemoveTokenAsync(IdentityUser user, string loginProvider, string name,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).RemoveToken(loginProvider, name);

        return Task.CompletedTask;
    }

    public Task SetAuthenticatorKeyAsync(IdentityUser user, string key,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).ReplaceToken(InternalLoginProvider, AuthenticatorKeyTokenName, key);

        return Task.CompletedTask;
    }

    public Task ReplaceCodesAsync(IdentityUser user, IEnumerable<string> recoveryCodes,
        CancellationToken cancellationToken)
    {
        ((MongoUser)user).ReplaceToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", recoveryCodes));

        return Task.CompletedTask;
    }

    public Task<bool> RedeemCodeAsync(IdentityUser user, string code,
        CancellationToken cancellationToken)
    {
        var mergedCodes = ((MongoUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName) ?? string.Empty;

        var splitCodes = mergedCodes.Split(';');
        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));

            ((MongoUser)user).ReplaceToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", updatedCodes));

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
