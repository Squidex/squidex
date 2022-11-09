// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;

namespace Squidex.Web.Pipeline;

public sealed class ContextFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        var requestContext =
            new Context(httpContext.User, null!).Clone(builder =>
            {
                foreach (var (key, value) in httpContext.Request.Headers)
                {
                    if (key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.SetHeader(key, value.ToString());
                    }
                }
            });

        httpContext.Features.Set(requestContext);

        return next();
    }
}
