// ==========================================================================
//  MustBeAppEditorAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Core.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppEditorAttribute : AuthorizeAttribute
    {
        public MustBeAppEditorAttribute()
        {
            Roles = $"{SquidexRoles.AppOwner},{SquidexRoles.AppDeveloper},{SquidexRoles.AppEditor}";
        }
    }
}
