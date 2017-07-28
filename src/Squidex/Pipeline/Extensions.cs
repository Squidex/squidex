// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using Squidex.Config;
using Squidex.Infrastructure.Security;

namespace Squidex.Pipeline
{
    public static class Extensions
    {
        public static bool IsFrontendClient(this ClaimsPrincipal principal)
        {
            return principal.IsInClient(Constants.FrontendClient);
        }
    }
}
