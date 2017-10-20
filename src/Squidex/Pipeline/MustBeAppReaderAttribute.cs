// ==========================================================================
//  MustBeAppReaderAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppReaderAttribute : AppPermissionAttribute
    {
        public MustBeAppReaderAttribute()
            : base(AppPermission.Reader)
        {
        }
    }
}
