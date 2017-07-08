// ==========================================================================
//  MustBeAdministratorAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAdministratorAttribute : AuthorizeAttribute
    {
        public MustBeAdministratorAttribute()
        {
            Roles = SquidexRoles.Administrator;
        }
    }
}
