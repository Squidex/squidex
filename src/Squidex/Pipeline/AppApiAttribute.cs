// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
