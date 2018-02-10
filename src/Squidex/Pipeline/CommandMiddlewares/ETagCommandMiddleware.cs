// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

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
            if (httpContextAccessor.HttpContext == null)
            {
                return;
            }

            var headers = httpContextAccessor.HttpContext.Request.Headers;
            var headerMatch = headers["If-Match"].ToString();

            if (!string.IsNullOrWhiteSpace(headerMatch) && long.TryParse(headerMatch, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedVersion))
            {
                context.Command.ExpectedVersion = expectedVersion;
            }
            else
            {
                context.Command.ExpectedVersion = EtagVersion.Any;
            }

            await next();

            if (context.Result<object>() is EntitySavedResult result)
            {
                httpContextAccessor.HttpContext.Response.Headers["ETag"] = result.Version.ToString();
            }
        }
    }
}
