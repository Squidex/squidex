// ==========================================================================
//  MustBeAppReaderAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppReaderAttribute : ApiAuthorizeAttribute
    {
        public MustBeAppReaderAttribute()
        {
            Roles = SquidexRoles.AppReader;
        }
    }
}
