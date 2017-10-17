// ==========================================================================
//  ApiAuthorizeAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;

namespace Squidex.Pipeline
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public ApiAuthorizeAttribute()
        {
            AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme;
        }
    }
}
