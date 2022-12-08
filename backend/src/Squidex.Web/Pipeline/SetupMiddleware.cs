// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Users;

namespace Squidex.Web.Pipeline;

public sealed class SetupMiddleware
{
    private readonly RequestDelegate next;
    private bool isUserFound;

    public SetupMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.Request.Query.ContainsKey("skip-setup"))
        {
            await next(context);
            return;
        }

        if (!isUserFound && await userService.IsEmptyAsync(context.RequestAborted))
        {
            var url = context.Request.PathBase.Add("/identity-server/setup");

            context.Response.Redirect(url);
        }
        else
        {
            isUserFound = true;

            await next(context);
        }
    }
}
