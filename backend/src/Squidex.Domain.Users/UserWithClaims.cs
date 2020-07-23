// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class UserWithClaims : IUser
    {
        public IdentityUser Identity { get; }

        public List<Claim> Claims { get; }

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

        IReadOnlyList<Claim> IUser.Claims
        {
            get { return Claims; }
        }

        public UserWithClaims(IdentityUser user, IEnumerable<Claim> claims)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNull(claims, nameof(claims));

            Identity = user;

            Claims = claims.ToList();
        }
    }
}
