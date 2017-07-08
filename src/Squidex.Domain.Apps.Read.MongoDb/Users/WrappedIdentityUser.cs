// ==========================================================================
//  WrappedIdentityUser.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Domain.Apps.Read.Users;

namespace Squidex.Domain.Apps.Read.MongoDb.Users
{
    public sealed class WrappedIdentityUser : IdentityUser, IUser
    {
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

        public void UpdateEmail(string email)
        {
            Email = UserName = email;
        }

        public void SetClaim(string type, string value)
        {
            Claims.RemoveAll(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
            Claims.Add(new IdentityUserClaim { Type = type, Value = value });
        }
    }
}
