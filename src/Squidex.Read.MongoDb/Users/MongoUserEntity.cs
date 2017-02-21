// ==========================================================================
//  MongoUserEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Core.Identity;
using Squidex.Read.Users;

namespace Squidex.Read.MongoDb.Users
{
    public class MongoUserEntity : IUserEntity
    {
        private readonly IdentityUser inner;

        public string Id
        {
            get { return inner.Id; }
        }

        public string Email
        {
            get { return inner.Email; }
        }

        public string DisplayName
        {
            get { return inner.Claims.Find(x => x.Type == SquidexClaimTypes.SquidexDisplayName)?.Value; }
        }

        public string PictureUrl
        {
            get { return inner.Claims.Find(x => x.Type == SquidexClaimTypes.SquidexPictureUrl)?.Value; }
        }

        public bool IsLocked
        {
            get { return inner.LockoutEndDateUtc != null && inner.LockoutEndDateUtc.Value > DateTime.UtcNow; }
        }

        public MongoUserEntity(IdentityUser inner)
        {
            this.inner = inner;
        }
    }
}
