// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Web.Pipeline;

public sealed class AppResolver(IAppProvider appProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (context.RouteData.Values.TryGetValue("app", out var appValue))
        {
            var appName = appValue?.ToString();
            if (string.IsNullOrWhiteSpace(appName))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var isFrontend = user.IsInClient(DefaultClients.Frontend);

            var app = await appProvider.GetAppAsync(appName, !isFrontend, context.HttpContext.RequestAborted);
            if (app == null)
            {
                var log = context.HttpContext.RequestServices?.GetService<ILogger<AppResolver>>();

                if (log != null)
                {
                    LogMessages.LogCannotFindAppByName(log, appName);
                }

                context.Result = new NotFoundResult();
                return;
            }

            AppPermissionClaims.Enrich(app, user);

            var requestContext = new Context(context.HttpContext.User, app).WithHeaders(context.HttpContext);

            if (!AllowAnonymous(context) && !AppPermissionClaims.HasAppPermission(appName, requestContext))
            {
                if (string.IsNullOrWhiteSpace(user.Identity?.AuthenticationType))
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    var log = context.HttpContext.RequestServices?.GetService<ILogger<AppResolver>>();

                    if (log != null)
                    {
                        LogMessages.LogNoPermissionToAccessApp(log, app.Id, app.Name);
                    }

                    context.Result = new NotFoundResult();
                }

                return;
            }

            context.HttpContext.Features.Set(requestContext);
            context.HttpContext.Features.Set(app);
            context.HttpContext.Response.Headers["X-AppId"] = app.Id.ToString();
        }

        await next();
    }

    private static bool AllowAnonymous(ActionExecutingContext context)
    {
        return context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute);
    }
}
