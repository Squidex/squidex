﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class OpenApiExtensions
    {
        public static void UseMyOpenApi(this IApplicationBuilder app)
        {
            app.UseOpenApi();
        }
    }
}
