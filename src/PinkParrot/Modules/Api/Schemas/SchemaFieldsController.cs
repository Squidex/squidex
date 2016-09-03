// ==========================================================================
//  SchemaFieldsController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasFieldsController : Controller
    {
        private readonly ICommandBus commandBus;

        public SchemasFieldsController(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        [HttpPost]
        [Route("schemas/{name}/fields/")]
        public Task Add(AddModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/")]
        public Task Update(UpdateModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/hide/")]
        public Task Hide(HideModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/show/")]
        public Task Show(ShowModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/enable/")]
        public Task Enable(EnableModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/disable/")]
        public Task Enable(DisableModelField command)
        {
            return commandBus.PublishAsync(command);
        }

        [HttpDelete]
        [Route("schemas/{name}/fields/{fieldId:long}/")]
        public Task Delete(DeleteModelField command)
        {
            return commandBus.PublishAsync(command);
        }
    }
}