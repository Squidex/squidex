// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
