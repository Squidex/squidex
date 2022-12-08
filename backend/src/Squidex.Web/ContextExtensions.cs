// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using RequestContext = Squidex.Domain.Apps.Entities.Context;

namespace Squidex.Web;

public static class ContextExtensions
{
    public static RequestContext Context(this HttpContext httpContext)
    {
        var context = httpContext.Features.Get<RequestContext>();

        if (context == null)
        {
            context = new RequestContext(httpContext.User, null!).WithHeaders(httpContext);

            httpContext.Features.Set(context);
        }

        return context;
    }

    public static RequestContext WithHeaders(this RequestContext context, HttpContext httpContext)
    {
        return context.Clone(builder =>
        {
            foreach (var (key, value) in httpContext.Request.Headers)
            {
                if (key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                {
                    builder.SetHeader(key, value.ToString());
                }
            }
        });
    }
}
