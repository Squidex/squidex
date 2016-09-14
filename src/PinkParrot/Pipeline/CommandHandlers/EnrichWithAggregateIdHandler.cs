// ==========================================================================
//  EnrichWithAggregateIdHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Read.Services;
using PinkParrot.Write;
using PinkParrot.Write.Schemas;

// ReSharper disable InvertIf

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class EnrichWithAggregateIdHandler : ICommandHandler
    {
        private readonly ISchemaProvider schemaProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithAggregateIdHandler(ISchemaProvider schemaProvider, IActionContextAccessor actionContextAccessor)
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
            
            var tenantCommand = context.Command as ITenantCommand;

            if (tenantCommand == null)
            {
                return false;
            }

            var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

            if (routeValues.ContainsKey("name"))
            {
                var schemaName = routeValues["name"].ToString();

                var id = await schemaProvider.FindSchemaIdByNameAsync(tenantCommand.TenantId, schemaName);

                if (!id.HasValue)
                {
                    throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                }

                aggregateCommand.AggregateId = id.Value;
            }

            return false;
        }
    }
}
