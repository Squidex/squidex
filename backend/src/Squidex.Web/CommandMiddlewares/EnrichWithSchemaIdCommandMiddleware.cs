// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares
{
    public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithSchemaIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return next(context);
            }

            if (context.Command is ISchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                var schemaId = GetSchemaId();

                schemaCommand.SchemaId = schemaId;
            }

            return next(context);
        }

        private NamedId<DomainId> GetSchemaId()
        {
            var feature = httpContextAccessor.HttpContext?.Features.Get<ISchemaFeature>();

            if (feature == null)
            {
                throw new InvalidOperationException("Cannot resolve schema.");
            }

            return feature.Schema.NamedId();
        }
    }
}
