// ==========================================================================
//  MongoUserEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Infrastructure.Security;
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
            get { return inner.Claims.Find(x => x.Type == ExtendedClaimTypes.SquidexDisplayName)?.Value; }
        }

        public string PictureUrl
        {
            get { return inner.Claims.Find(x => x.Type == ExtendedClaimTypes.SquidexPictureUrl)?.Value; }
        }

        public MongoUserEntity(IdentityUser inner)
        {
            this.inner = inner;
        }
    }
}
