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
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Read.Services;

// ReSharper disable InvertIf

namespace PinkParrot.Pipeline.CommandHandlers
{
    public sealed class EnrichWithAggregateIdHandler : ICommandHandler
    {
        private readonly IModelSchemaProvider modelSchemaProvider;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithAggregateIdHandler(IModelSchemaProvider modelSchemaProvider, IActionContextAccessor actionContextAccessor)
        {
            this.modelSchemaProvider = modelSchemaProvider;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task<bool> HandleAsync(CommandContext context)
        {
            var aggregateCommand = context.Command as IAggregateCommand;

            if (aggregateCommand != null && aggregateCommand.AggregateId == Guid.Empty)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemeName = routeValues["name"];

                    aggregateCommand.AggregateId = await modelSchemaProvider.FindSchemaIdByNameAsync(schemeName.ToString());
                }
            }

            return false;
        }
    }
}
