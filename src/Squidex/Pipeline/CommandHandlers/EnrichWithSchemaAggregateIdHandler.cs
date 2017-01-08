// ==========================================================================
//  EnrichWithSchemaAggregateIdHandler.cs
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
    public sealed class EnrichWithSchemaAggregateIdHandler : ICommandHandler
    {
        private readonly ISchemaProvider schemaProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaAggregateIdHandler(ISchemaProvider schemaProvider, IActionContextAccessor actionContextAccessor)
        {
            this.schemaProvider = schemaProvider;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task<bool> HandleAsync(CommandContext context)
        {
            var aggregateCommand = context.Command as IAggregateCommand;

            if (aggregateCommand == null || aggregateCommand.AggregateId != Guid.Empty)
            {
                return false;
            }
            
            var appCommand = context.Command as IAppCommand;

            if (appCommand == null)
            {
                return false;
            }

            var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

            if (routeValues.ContainsKey("name"))
            {
                var schemaName = routeValues["name"].ToString();

                var schema = await schemaProvider.ProvideSchemaByNameAsync(appCommand.AppId, schemaName);

                if (schema == null)
                {
                    throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                }

                aggregateCommand.AggregateId = schema.Id;
            }

            return false;
        }
    }
}
