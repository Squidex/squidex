// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Areas.Mcp.Tools;
using Squidex.Web;

namespace Squidex.Areas.Mcp.Config;

public static class McpServices
{
    public static void AddSquidexMcp(this IServiceCollection services)
    {
        services.AddMcpServer(options =>
            {
                options.ServerInfo = new () { Name = "Squidex", Version = typeof(McpServices).Assembly.GetName().Version?.ToString() ?? "1.0" };
            })
            .WithHttpTransport()
            .WithTools<SchemaTools>()
            .WithTools<ContentTools>()
            .WithTools<AssetTools>();
    }

    public static void UseSquidexMcp(this IApplicationBuilder app)
    {
        app.UseMiddleware<McpAppMiddleware>();
    }

    public static void MapSquidexMcp(this IEndpointRouteBuilder endpoints)
    {
        // Authenticate with the API scheme, but allow anonymous clients. Permissions are checked by the middleware.
        endpoints.MapMcp($"{Constants.PrefixApi}/apps/{{app}}/mcp")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = Constants.ApiSecurityScheme })
            .AllowAnonymous()
            .WithMetadata(new McpEndpointMetadata());
    }
}
