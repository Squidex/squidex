// ==========================================================================
//  MongoUserStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using MongoDB.Driver;
using Squidex.Read.Users;

namespace Squidex.Read.MongoDb.Users
{
    public sealed class MongoUserStore :
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
        IQueryableUserStore<IUser>
    {
        private readonly UserStore<WrappedIdentityUser> innerStore;

        public MongoUserStore(IMongoDatabase database)
        {
            var usersCollection = database.GetCollection<WrappedIdentityUser>("Identity_Users");

            IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
            IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

            innerStore = new UserStore<WrappedIdentityUser>(usersCollection);
        }

        public void Dispose()
        {
            innerStore.Dispose();
        }

        public IQueryable<IUser> Users
        {
            get { return innerStore.Users; }
        }

        public IUser Create(string email)
        {
            return new WrappedIdentityUser { Email = email, UserName = email };
        }

        public async Task<IUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await innerStore.FindByIdAsync(userId, cancellationToken);
        }

        public async Task<IUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await innerStore.FindByEmailAsync(normalizedEmail, cancellationToken);
        }

        public async Task<IUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await innerStore.FindByNameAsync(normalizedUserName, cancellationToken);
        }

        public async Task<IUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await innerStore.FindByLoginAsync(loginProvider, providerKey, cancellationToken);
        }

        public async Task<IList<IUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            return (await innerStore.GetUsersForClaimAsync(claim, cancellationToken)).OfType<IUser>().ToList();
        }

        public async Task<IList<IUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return (await innerStore.GetUsersInRoleAsync(roleName, cancellationToken)).OfType<IUser>().ToList();
        }

        public Task<IdentityResult> CreateAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.CreateAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<IdentityResult> UpdateAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.UpdateAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<IdentityResult> DeleteAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.DeleteAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<string> GetUserIdAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetUserIdAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<string> GetUserNameAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetUserNameAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetUserNameAsync(IUser user, string userName, CancellationToken cancellationToken)
        {
            return innerStore.SetUserNameAsync((WrappedIdentityUser)user, userName, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetNormalizedUserNameAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetNormalizedUserNameAsync(IUser user, string normalizedName, CancellationToken cancellationToken)
        {
            return innerStore.SetNormalizedUserNameAsync((WrappedIdentityUser)user, normalizedName, cancellationToken);
        }

        public Task<string> GetPasswordHashAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetPasswordHashAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetPasswordHashAsync(IUser user, string passwordHash, CancellationToken cancellationToken)
        {
            return innerStore.SetPasswordHashAsync((WrappedIdentityUser)user, passwordHash, cancellationToken);
        }

        public Task<bool> HasPasswordAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.HasPasswordAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task AddToRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            return innerStore.AddToRoleAsync((WrappedIdentityUser)user, roleName, cancellationToken);
        }

        public Task RemoveFromRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            return innerStore.RemoveFromRoleAsync((WrappedIdentityUser)user, roleName, cancellationToken);
        }

        public Task<IList<string>> GetRolesAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetRolesAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<bool> IsInRoleAsync(IUser user, string roleName, CancellationToken cancellationToken)
        {
            return innerStore.IsInRoleAsync((WrappedIdentityUser)user, roleName, cancellationToken);
        }

        public Task AddLoginAsync(IUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            return innerStore.AddLoginAsync((WrappedIdentityUser)user, login, cancellationToken);
        }

        public Task RemoveLoginAsync(IUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return innerStore.RemoveLoginAsync((WrappedIdentityUser)user, loginProvider, providerKey, cancellationToken);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetLoginsAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<string> GetSecurityStampAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetSecurityStampAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetSecurityStampAsync(IUser user, string stamp, CancellationToken cancellationToken)
        {
            return innerStore.SetSecurityStampAsync((WrappedIdentityUser)user, stamp, cancellationToken);
        }

        public Task<string> GetEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetEmailAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetEmailAsync(IUser user, string email, CancellationToken cancellationToken)
        {
            return innerStore.SetEmailAsync((WrappedIdentityUser)user, email, cancellationToken);
        }

        public Task<bool> GetEmailConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetEmailConfirmedAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetEmailConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return innerStore.SetEmailConfirmedAsync((WrappedIdentityUser)user, confirmed, cancellationToken);
        }

        public Task<string> GetNormalizedEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetNormalizedEmailAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetNormalizedEmailAsync(IUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return innerStore.SetNormalizedEmailAsync((WrappedIdentityUser)user, normalizedEmail, cancellationToken);
        }

        public Task<IList<Claim>> GetClaimsAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetClaimsAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task AddClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            return innerStore.AddClaimsAsync((WrappedIdentityUser)user, claims, cancellationToken);
        }

        public Task ReplaceClaimAsync(IUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            return innerStore.ReplaceClaimAsync((WrappedIdentityUser)user, claim, newClaim, cancellationToken);
        }

        public Task RemoveClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            return innerStore.RemoveClaimsAsync((WrappedIdentityUser)user, claims, cancellationToken);
        }

        public Task<string> GetPhoneNumberAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetPhoneNumberAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetPhoneNumberAsync(IUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            return innerStore.SetPhoneNumberAsync((WrappedIdentityUser)user, phoneNumber, cancellationToken);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetPhoneNumberConfirmedAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetPhoneNumberConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return innerStore.SetPhoneNumberConfirmedAsync((WrappedIdentityUser)user, confirmed, cancellationToken);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetTwoFactorEnabledAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetTwoFactorEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            return innerStore.SetTwoFactorEnabledAsync((WrappedIdentityUser)user, enabled, cancellationToken);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetLockoutEndDateAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            return innerStore.SetLockoutEndDateAsync((WrappedIdentityUser)user, lockoutEnd, cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetAccessFailedCountAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<int> IncrementAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.IncrementAccessFailedCountAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task ResetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.ResetAccessFailedCountAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task<bool> GetLockoutEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            return innerStore.GetLockoutEnabledAsync((WrappedIdentityUser)user, cancellationToken);
        }

        public Task SetLockoutEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            return innerStore.SetLockoutEnabledAsync((WrappedIdentityUser)user, enabled, cancellationToken);
        }

        public Task SetTokenAsync(IUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            return innerStore.SetTokenAsync((WrappedIdentityUser)user, loginProvider, name, value, cancellationToken);
        }

        public Task RemoveTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return innerStore.RemoveTokenAsync((WrappedIdentityUser)user, loginProvider, name, cancellationToken);
        }

        public Task<string> GetTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return innerStore.GetTokenAsync((WrappedIdentityUser)user, loginProvider, name, cancellationToken);
        }
    }
}
