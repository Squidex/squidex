// ==========================================================================
//  EnrichWithSchemaIdHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public sealed class EnrichWithSchemaIdCommandHandler : ICommandHandler
    {
        private readonly ISchemaProvider schemas;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdCommandHandler(ISchemaProvider schemas, IActionContextAccessor actionContextAccessor)
        {
            this.schemas = schemas;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is SchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    ISchemaEntity schema;

                    if (Guid.TryParse(schemaName, out var id))
                    {
                        schema = await schemas.FindSchemaByIdAsync(id);
                    }
                    else
                    {
                        schema = await schemas.FindSchemaByNameAsync(schemaCommand.AppId.Id, schemaName);
                    }

                    if (schema == null)
                    {
                        throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                    }

                    schemaCommand.SchemaId = new NamedId<Guid>(schema.Id, schema.Name);
                }
            }

            await next();
        }
    }
}
