﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares
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
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                await next();

                return;
            }

            var command = context.Command;

            if (command.ExpectedVersion == EtagVersion.Auto)
            {
                if (TryParseEtag(httpContext, out var expectedVersion))
                {
                    command.ExpectedVersion = expectedVersion;
                }
                else
                {
                    command.ExpectedVersion = EtagVersion.Any;
                }
            }

            await next();

            if (context.PlainResult is EntitySavedResult result)
            {
                SetResponsEtag(httpContext, result.Version);
            }
            else if (context.PlainResult is IEntityWithVersion entity)
            {
                SetResponsEtag(httpContext, entity.Version);
            }
        }

        private static void SetResponsEtag(HttpContext httpContext, long version)
        {
            httpContext.Response.Headers[HeaderNames.ETag] = version.ToString();
        }

        private static bool TryParseEtag(HttpContext httpContext, out long version)
        {
            version = default;

            if (httpContext.Request.Headers.TryGetHeaderString(HeaderNames.IfMatch, out var etag))
            {
                if (etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                {
                    etag = etag.Substring(2);
                }

                if (long.TryParse(etag, NumberStyles.Any, CultureInfo.InvariantCulture, out version))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
