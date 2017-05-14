// ==========================================================================
//  AppApiAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;

namespace Squidex.Pipeline
{
    public sealed class AppApiAttribute : ServiceFilterAttribute
    {
        public AppApiAttribute()
            : base(typeof(AppApiFilter))
        {
        }
    }
}
