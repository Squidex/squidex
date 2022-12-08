// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public EnrichWithSchemaIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            return next(context, ct);
        }

        if (context.Command is ISchemaCommand { SchemaId: null } schemaCommand)
        {
            var schemaId = GetSchemaId();

            schemaCommand.SchemaId = schemaId;
        }

        return next(context, ct);
    }

    private NamedId<DomainId> GetSchemaId()
    {
        var feature = httpContextAccessor.HttpContext?.Features.Get<ISchemaFeature>();

        if (feature == null)
        {
            ThrowHelper.InvalidOperationException("Cannot resolve schema.");
            return default!;
        }

        return feature.Schema.NamedId();
    }
}
