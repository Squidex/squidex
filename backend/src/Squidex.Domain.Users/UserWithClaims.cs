// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    internal sealed class UserWithClaims : IUser
    {
        public IdentityUser Identity { get; }

        public string Id
        {
            get { return Identity.Id; }
        }

        public string Email
        {
            get { return Identity.Email; }
        }

        public bool IsLocked
        {
            get { return Identity.LockoutEnd > DateTime.UtcNow; }
        }

        public IReadOnlyList<Claim> Claims { get; }

        object IUser.Identity => Identity;

        public UserWithClaims(IdentityUser user, IReadOnlyList<Claim> claims)
        {
            Identity = user;

            Claims = claims;
        }
    }
}
