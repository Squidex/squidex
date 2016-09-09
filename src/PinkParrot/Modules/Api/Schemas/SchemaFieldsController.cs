// ==========================================================================
//  SchemaFieldsController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Write.Schema.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasFieldsController : ControllerBase
    {
        public SchemasFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }
        
        [HttpPost]
        [Route("api/schemas/{name}/fields/")]
        public Task Add(string name, [FromBody] ModelFieldProperties field)
        {
            var command = new AddModelField { Properties = field };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/")]
        public Task Update(string name, long fieldId, [FromBody] ModelFieldProperties properties)
        {
            var command = new UpdateModelField { FieldId = fieldId, Properties = properties };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/hide/")]
        public Task Hide(string name, long fieldId)
        {
            var command = new HideModelField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/show/")]
        public Task Show(string name, long fieldId)
        {
            var command = new ShowModelField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/enable/")]
        public Task Enable(string name, long fieldId)
        {
            var command = new EnableModelField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/disable/")]
        public Task Disable(string name, long fieldId)
        {
            var command = new DisableModelField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpDelete]
        [Route("api/schemas/{name}/fields/{fieldId:long}/")]
        public Task Delete(string name, long fieldId)
        {
            var command = new DeleteModelField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
    }
}