// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Frontend.Middlewares;

public sealed class EmbedMiddleware
{
    private readonly RequestDelegate next;

    public EmbedMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        if (request.Path.StartsWithSegments("/embed", StringComparison.Ordinal, out var remaining))
        {
            request.Path = remaining;

            var uiOptions = new OptionsFeature();
            uiOptions.Options["embedded"] = true;
            uiOptions.Options["embedPath"] = "/embed";

            context.Features.Set(uiOptions);
        }

        return next(context);
    }
}
