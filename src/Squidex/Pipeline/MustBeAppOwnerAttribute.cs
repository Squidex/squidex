// ==========================================================================
//  MustBeAppOwnerAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppOwnerAttribute : AppPermissionAttribute
    {
        public MustBeAppOwnerAttribute()
            : base(AppPermission.Owner)
        {
        }
    }
}
