// ==========================================================================
//  MongoUserLogin.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUserLogin
    {
        [BsonRequired]
        [BsonElement]
        public string LoginProvider { get; set; }

        [BsonRequired]
        [BsonElement]
        public string ProviderDisplayName { get; set; }

        [BsonRequired]
        [BsonElement]
        public string ProviderKey { get; set; }

        public static implicit operator MongoUserLogin(UserLoginInfo login)
        {
            return new MongoUserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName
            };
        }

        public static implicit operator UserLoginInfo(MongoUserLogin userLogin)
        {
            return new UserLoginInfo(userLogin.LoginProvider, userLogin.ProviderKey, userLogin.ProviderDisplayName);
        }
    }
}
