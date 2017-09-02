// ==========================================================================
//  ETagCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class ETagCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ETagCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            var headers = httpContextAccessor.HttpContext.Request.Headers;
            var headerMatch = headers["If-Match"].ToString();

            if (!string.IsNullOrWhiteSpace(headerMatch) && long.TryParse(headerMatch, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedVersion))
            {
                context.Command.ExpectedVersion = expectedVersion;
            }

            await next();

            if (context.Result<object>() is EntitySavedResult result)
            {
                httpContextAccessor.HttpContext.Response.Headers["ETag"] = new StringValues(result.Version.ToString());
            }
        }
    }
}
