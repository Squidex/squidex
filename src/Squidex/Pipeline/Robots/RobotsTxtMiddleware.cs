// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;

namespace Squidex.Pipeline.Robots
{
    public sealed class RobotsTxtMiddleware : IMiddleware
    {
        private readonly RobotsTxtOptions robotsTxtOptions;

        public RobotsTxtMiddleware(IOptions<RobotsTxtOptions> robotsTxtOptions)
        {
            Guard.NotNull(robotsTxtOptions, nameof(robotsTxtOptions));

            this.robotsTxtOptions = robotsTxtOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (CanServeRequest(context.Request) && !string.IsNullOrWhiteSpace(robotsTxtOptions.Text))
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 200;

                await context.Response.WriteAsync(robotsTxtOptions.Text);
            }
            else
            {
                await next(context);
            }
        }

        private static bool CanServeRequest(HttpRequest request)
        {
            return HttpMethods.IsGet(request.Method) && string.IsNullOrEmpty(request.Path);
        }
    }
}
