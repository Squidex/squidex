// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares
{
    public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithSchemaIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor, nameof(httpContextAccessor));

            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is ISchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                var schemaId = GetSchemaId();

                schemaCommand.SchemaId = schemaId!;
            }

            if (context.Command is SchemaCommand schemaSelfCommand && schemaSelfCommand.SchemaId == Guid.Empty)
            {
                var schemaId = GetSchemaId();

                schemaSelfCommand.SchemaId = schemaId?.Id ?? Guid.Empty;
            }

            await next(context);
        }

        private NamedId<Guid> GetSchemaId()
        {
            var feature = httpContextAccessor.HttpContext.Features.Get<ISchemaFeature>();

            if (feature == null)
            {
                throw new InvalidOperationException("Cannot resolve schema.");
            }

            return feature.SchemaId;
        }
    }
}