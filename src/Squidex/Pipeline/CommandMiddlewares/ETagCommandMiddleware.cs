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
using Microsoft.Net.Http.Headers;
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
                await next();

                return;
            }

            context.Command.ExpectedVersion = EtagVersion.Any;

            var headers = httpContextAccessor.HttpContext.Request.Headers;

            if (headers.TryGetValue(HeaderNames.IfMatch, out var etag) && !string.IsNullOrWhiteSpace(etag))
            {
                var etagValue = etag.ToString();

                if (etagValue.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                {
                    etagValue = etagValue.Substring(2);
                }

                if (long.TryParse(etagValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedVersion))
                {
                    context.Command.ExpectedVersion = expectedVersion;
                }
            }

            await next();

            if (context.PlainResult is EntitySavedResult result)
            {
                httpContextAccessor.HttpContext.Response.Headers[HeaderNames.ETag] = result.Version.ToString();
            }
        }
    }
}
