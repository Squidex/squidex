// ==========================================================================
//  MustBeAppDeveloperAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppDeveloperAttribute : ApiAuthorizeAttribute
    {
        public MustBeAppDeveloperAttribute()
        {
            Roles = SquidexRoles.AppDeveloper;
        }
    }
}
