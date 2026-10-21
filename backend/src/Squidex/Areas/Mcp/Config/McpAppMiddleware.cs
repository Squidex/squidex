// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Mcp.Config;

public sealed class McpAppMiddleware(RequestDelegate next)
{
    private static readonly ApiCostsAttribute Costs = new ApiCostsAttribute(1);

    public async Task InvokeAsync(HttpContext httpContext, IAppProvider appProvider, IUsageGate usageGate)
    {
        // The MCP endpoints are minimal API endpoints, therefore the MVC filters for apps and costs do not run.
        if (httpContext.GetEndpoint()?.Metadata.GetMetadata<McpEndpointMetadata>() == null)
        {
            await next(httpContext);
            return;
        }

        var appName = httpContext.GetRouteValue("app")?.ToString();
        if (string.IsNullOrWhiteSpace(appName))
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var user = httpContext.User;

        var app = await appProvider.GetAppAsync(appName, !user.IsInClient(DefaultClients.Frontend), httpContext.RequestAborted);
        if (app == null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        AppPermissionClaims.Enrich(app, user);

        var requestContext = new Context(user, app).WithHeaders(httpContext);

        if (!AppPermissionClaims.HasAppPermission(appName, requestContext))
        {
            var isAnonymous = string.IsNullOrWhiteSpace(user.Identity?.AuthenticationType);

            httpContext.Response.StatusCode = isAnonymous ? StatusCodes.Status401Unauthorized : StatusCodes.Status404NotFound;
            return;
        }

        if (await IsBlockedAsync(httpContext, app, usageGate))
        {
            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return;
        }

        httpContext.Features.Set(requestContext);
        httpContext.Features.Set(app);
        httpContext.Features.Set<IApiCostsFeature>(Costs);
        httpContext.Response.Headers["X-AppId"] = app.Id.ToString();
        httpContext.Response.Headers["X-Costs"] = Costs.Costs.ToString(CultureInfo.InvariantCulture);

        await next(httpContext);
    }

    private static Task<bool> IsBlockedAsync(HttpContext httpContext, App app, IUsageGate usageGate)
    {
        var (_, clientId) = httpContext.User.GetClient();

        return usageGate.IsBlockedAsync(app, clientId, DateTime.Today.ToDateOnly(), httpContext.RequestAborted);
    }
}
