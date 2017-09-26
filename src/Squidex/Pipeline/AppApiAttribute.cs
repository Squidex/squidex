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
        public bool CheckPermissions { get; }

        public AppApiAttribute(bool checkPermissions = true)
            : base(typeof(AppApiFilter))
        {
            CheckPermissions = checkPermissions;
        }
    }
}
