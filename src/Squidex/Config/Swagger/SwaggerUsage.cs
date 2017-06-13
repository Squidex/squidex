// ==========================================================================
//  SwaggerUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;

namespace Squidex.Config.Swagger
{
    public static class SwaggerUsage
    {
        public static void UseMySwagger(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<SwaggerSettings>();

            app.UseSwagger(typeof(SwaggerUsage).GetTypeInfo().Assembly, settings);
        }
    }
}
