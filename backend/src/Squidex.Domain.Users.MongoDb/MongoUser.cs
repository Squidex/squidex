// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUser : IdentityUser
    {
        [BsonRequired]
        [BsonElement]
        public List<Claim> Claims { get; set; } = new List<Claim>();

        [BsonRequired]
        [BsonElement]
        public List<UserTokenInfo> Tokens { get; set; } = new List<UserTokenInfo>();

        [BsonRequired]
        [BsonElement]
        public List<UserLogin> Logins { get; set; } = new List<UserLogin>();

        [BsonRequired]
        [BsonElement]
        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        internal string? GetToken(string provider, string name)
        {
            return Tokens.Find(x => x.LoginProvider == provider && x.Name == name)?.Value;
        }

        internal void AddLogin(UserLoginInfo login)
        {
            Logins.Add(new UserLogin(login));
        }

        internal void AddRole(string role)
        {
            Roles.Add(role);
        }

        internal void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        internal void AddClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(x => AddClaim(x));
        }

        internal void AddToken(string provider, string name, string value)
        {
            Tokens.Add(new UserTokenInfo { LoginProvider = provider, Name = name, Value = value });
        }

        internal void RemoveLogin(string provider, string providerKey)
        {
            Logins.RemoveAll(x => x.LoginProvider == provider && x.ProviderKey == providerKey);
        }

        internal void RemoveClaim(Claim claim)
        {
            Claims.RemoveAll(x => x.Type == claim.Type && x.Value == claim.Value);
        }

        internal void RemoveToken(string provider, string name)
        {
            Tokens.RemoveAll(x => x.LoginProvider == provider && x.Name == name);
        }

        internal void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        internal void RemoveClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(x => RemoveClaim(x));
        }

        internal void ReplaceClaim(Claim existingClaim, Claim newClaim)
        {
            RemoveClaim(existingClaim);

            AddClaim(newClaim);
        }

        internal void ReplaceToken(string provider, string name, string value)
        {
            RemoveToken(provider, name);

            AddToken(provider, name, value);
        }
    }

    public sealed class UserTokenInfo : IdentityUserToken<string>
    {
    }

    public sealed class UserLogin : UserLoginInfo
    {
        public UserLogin(string provider, string providerKey, string displayName)
            : base(provider, providerKey, displayName)
        {
        }

        public UserLogin(UserLoginInfo source)
            : base(source.LoginProvider, source.ProviderKey, source.ProviderDisplayName)
        {
        }
    }
}
