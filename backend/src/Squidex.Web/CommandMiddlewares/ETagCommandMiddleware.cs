// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public class ETagCommandMiddleware : ICommandMiddleware
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public ETagCommandMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            await next(context, ct);
            return;
        }

        var command = context.Command;

        if (command.ExpectedVersion == EtagVersion.Auto)
        {
            if (httpContext.TryParseEtagVersion(HeaderNames.IfMatch, out var expectedVersion))
            {
                command.ExpectedVersion = expectedVersion;
            }
            else
            {
                command.ExpectedVersion = EtagVersion.Any;
            }
        }

        await next(context, ct);

        if (context.PlainResult is CommandResult result)
        {
            SetResponsEtag(httpContext, result.NewVersion);
        }
        else if (context.PlainResult is IEntityWithVersion entity)
        {
            SetResponsEtag(httpContext, entity.Version);
        }
    }

    private static void SetResponsEtag(HttpContext httpContext, long version)
    {
        httpContext.Response.Headers[HeaderNames.ETag] = version.ToString(CultureInfo.InvariantCulture);
    }
}
