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
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUser : IUser
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement]
        public string Id { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string SecurityStamp { get; set; }

        [BsonRequired]
        [BsonElement]
        public string UserName { get; set; }

        [BsonRequired]
        [BsonElement]
        public string NormalizedUserName { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Email { get; set; }

        [BsonRequired]
        [BsonElement]
        public string NormalizedEmail { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string PhoneNumber { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string PasswordHash { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool EmailConfirmed { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public bool PhoneNumberConfirmed { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public bool TwoFactorEnabled { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public bool LockoutEnabled { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public DateTime? LockoutEndDateUtc { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public int AccessFailedCount { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<string> Roles { get; set; } = new List<string>();

        [BsonRequired]
        [BsonElement]
        public List<MongoUserClaim> Claims { get; set; } = new List<MongoUserClaim>();

        [BsonRequired]
        [BsonElement]
        public List<MongoUserToken> Tokens { get; set; } = new List<MongoUserToken>();

        [BsonRequired]
        [BsonElement]
        public List<MongoUserLogin> Logins { get; set; } = new List<MongoUserLogin>();

        public bool IsLocked
        {
            get { return LockoutEndDateUtc != null && LockoutEndDateUtc.Value > DateTime.UtcNow; }
        }

        IReadOnlyList<Claim> IUser.Claims
        {
            get { return Claims.Select(x => new Claim(x.Type, x.Value)).ToList(); }
        }

        IReadOnlyList<ExternalLogin> IUser.Logins
        {
            get { return Logins.Select(x => new ExternalLogin(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList(); }
        }

        public MongoUser()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }

        public void SetEmail(string email)
        {
            Email = UserName = email;
        }

        public void AddRole(string role)
        {
            Roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        public void AddLogin(UserLoginInfo login)
        {
            Logins.Add(login);
        }

        public void RemoveLogin(string loginProvider, string providerKey)
        {
            Logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        }

        public void AddClaim(Claim claim)
        {
            Claims.Add(claim);
        }

        public void AddClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(AddClaim);
        }

        public void RemoveClaim(Claim claim)
        {
            Claims.RemoveAll(c => c.Type == claim.Type && c.Value == claim.Value);
        }

        public void RemoveClaims(IEnumerable<Claim> claims)
        {
            claims.Foreach(RemoveClaim);
        }

        public string GetToken(string loginProider, string name)
        {
            return Tokens.FirstOrDefault(t => t.LoginProvider == loginProider && t.Name == name)?.Value;
        }

        public void AddToken(string loginProvider, string name, string value)
        {
            Tokens.Add(new MongoUserToken { LoginProvider = loginProvider, Name = name, Value = value });
        }

        public void RemoveToken(string loginProvider, string name)
        {
            Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
        }

        public void SetClaim(string type, string value)
        {
            Claims.RemoveAll(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

            AddClaim(new Claim(type, value));
        }

        public void ReplaceClaim(Claim existingClaim, Claim newClaim)
        {
            RemoveClaim(existingClaim);

            AddClaim(newClaim);
        }

        public void SetToken(string loginProider, string name, string value)
        {
            RemoveToken(loginProider, name);

            AddToken(loginProider, name, value);
        }
    }
}
