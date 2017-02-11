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
        private readonly ISchemaProvider schemaProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdHandler(ISchemaProvider schemaProvider, IActionContextAccessor actionContextAccessor)
        {
            this.schemaProvider = schemaProvider;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task<bool> HandleAsync(CommandContext context)
        {
            var schemaCommand = context.Command as SchemaCommand;

            if (schemaCommand != null)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    var schema = await schemaProvider.FindSchemaByNameAsync(schemaCommand.AppId.Id, schemaName);

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
