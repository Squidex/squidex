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
        public List<UserLoginInfo> Logins { get; set; } = new List<UserLoginInfo>();

        [BsonRequired]
        [BsonElement]
        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        internal void AddLogin(UserLoginInfo login)
        {
            Logins.Add(login);
        }

        internal void AddRole(string role)
        {
            Roles.Add(role);
        }

        internal void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        internal void RemoveLogin(string provider, string providerKey)
        {
            Logins.RemoveAll(x => x.LoginProvider == provider && x.ProviderKey == providerKey);
        }

        internal void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        internal void AddClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach((x, _) => AddClaim(x));
        }

        internal void RemoveClaim(Claim claim)
        {
            Claims.RemoveAll(x => x.Type == claim.Type && x.Value == claim.Value);
        }

        internal void RemoveClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach((x, _) => RemoveClaim(x));
        }

        internal string? GetToken(string provider, string name)
        {
            return Tokens.FirstOrDefault(x => x.LoginProvider == provider && x.Name == name)?.Value;
        }

        internal void AddToken(string provider, string name, string value)
        {
            Tokens.Add(new UserTokenInfo { LoginProvider = provider, Name = name, Value = value });
        }

        internal void RemoveToken(string provider, string name)
        {
            Tokens.RemoveAll(x => x.LoginProvider == provider && x.Name == name);
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
