// ==========================================================================
//  MustBeAppReaderAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppReaderAttribute : AuthorizeAttribute
    {
        public MustBeAppReaderAttribute()
        {
            Roles = SquidexRoles.AppReader;
        }
    }
}
