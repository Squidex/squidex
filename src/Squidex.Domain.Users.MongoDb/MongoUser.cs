// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUser : IdentityUser
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();

        public List<UserTokenInfo> Tokens { get; set; } = new List<UserTokenInfo>();

        public List<UserLoginInfo> Logins { get; set; } = new List<UserLoginInfo>();

        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        internal IdentityUserToken<string> FindTokenAsync(string loginProvider, string name)
        {
            return Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);
        }

        internal void AddLogin(UserLoginInfo login)
        {
            Logins.Add(new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName));
        }

        internal void AddRole(string role)
        {
            Roles.Add(role);
        }

        internal void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        internal void RemoveLogin(string loginProvider, string providerKey)
        {
            Logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        }

        internal void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        internal void AddClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(AddClaim);
        }

        internal void RemoveClaim(Claim claim)
        {
            Claims.RemoveAll(c => c.Type == claim.Type && c.Value == claim.Value);
        }

        internal void RemoveClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(RemoveClaim);
        }

        internal string GetToken(string loginProvider, string name)
        {
            return Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name)?.Value;
        }

        internal void AddToken(string loginProvider, string name, string value)
        {
            Tokens.Add(new UserTokenInfo { LoginProvider = loginProvider, Name = name, Value = value });
        }

        internal void RemoveToken(string loginProvider, string name)
        {
            Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
        }

        internal void ReplaceClaim(Claim existingClaim, Claim newClaim)
        {
            RemoveClaim(existingClaim);

            AddClaim(newClaim);
        }

        internal void SetToken(string loginProider, string name, string value)
        {
            RemoveToken(loginProider, name);

            AddToken(loginProider, name, value);
        }
    }

    public sealed class UserTokenInfo : IdentityUserToken<string>
    {
    }
}
