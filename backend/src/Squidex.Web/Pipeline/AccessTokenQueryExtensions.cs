// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Web.Pipeline;

public static class AccessTokenQueryExtensions
{
    public static void UseAccessTokenQueryString(this IApplicationBuilder app)
    {
        app.UseMiddleware<AccessTokenQueryMiddleware>();
    }
}
