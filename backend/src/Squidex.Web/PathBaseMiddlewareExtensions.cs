// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;

namespace Squidex.Web;

public static class PathBaseMiddlewareExtensions
{
    public static IApplicationBuilder UseFallbackPathBase(this IApplicationBuilder app)
    {
        var urlGenerator = app.ApplicationServices.GetRequiredService<IUrlGenerator>();

        var configuredBase = urlGenerator.BuildBasePath();
        if (string.IsNullOrWhiteSpace(configuredBase))
        {
            return app;
        }

        var pathBase = new PathString(configuredBase);

        return app.Use(async (context, next) =>
        {
            if (context.Request.PathBase.HasValue)
            {
                await next();
                return;
            }

            context.Request.PathBase = pathBase;

            if (context.Request.Path.StartsWithSegments(pathBase, StringComparison.Ordinal, out var remainder))
            {
                context.Request.Path = remainder;
            }

            await next();
        });
    }
}
