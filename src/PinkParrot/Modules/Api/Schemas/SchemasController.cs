// ==========================================================================
//  SchemasController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Read.Models;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasController : Controller
    {
        private readonly ICommandBus commandBus;

        public SchemasController(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        [HttpGet]
        [Route("schemas/")]
        public Task<List<ModelSchemaRM>> Query()
        {
            return null;
        }

        [HttpPost]
        [Route("schemas/")]
        public Task Create(CreateModelSchema command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/")]
        public Task Update(UpdateModelSchema command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpDelete]
        [Route("schemas/{name}/")]
        public Task Delete()
        {
            return commandBus.PublishAsync(new DeleteModelSchema());
        }
    }
}