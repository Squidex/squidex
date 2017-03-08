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
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Read.Schemas.Services;
using Squidex.Write;
using Squidex.Write.Schemas;

// ReSharper disable InvertIf

namespace Squidex.Pipeline.CommandHandlers
{
    public sealed class EnrichWithSchemaIdHandler : ICommandHandler
    {
        private readonly ISchemaProvider schemas;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdHandler(ISchemaProvider schemas, IActionContextAccessor actionContextAccessor)
        {
            this.schemas = schemas;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task<bool> HandleAsync(CommandContext context)
        {
            if (context.Command is SchemaCommand schemaCommand)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    var schema = await schemas.FindSchemaByNameAsync(schemaCommand.AppId.Id, schemaName);

                    if (schema == null)
                    {
                        throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                    }

                    schemaCommand.SchemaId = new NamedId<Guid>(schema.Id, schema.Name);
                }
            }

            return false;
        }
    }
}
