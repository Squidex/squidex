// ==========================================================================
//  MustBeAppDeveloperAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Core.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppDeveloperAttribute : AuthorizeAttribute
    {
        public MustBeAppDeveloperAttribute()
        {
            Roles = SquidexRoles.AppDeveloper;
        }
    }
}
