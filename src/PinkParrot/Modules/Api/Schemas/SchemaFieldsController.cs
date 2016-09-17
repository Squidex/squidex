// ==========================================================================
//  SchemaFieldsController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Modules.Api.Schemas.Models;
using PinkParrot.Pipeline;
using PinkParrot.Write.Schemas.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    [ApiExceptionFilter]
    public class SchemasFieldsController : ControllerBase
    {
        public SchemasFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }
        
        [HttpPost]
        [Route("api/schemas/{name}/fields/")]
        public Task Add(string name, [FromBody] CreateFieldDto model)
        {
            var command = SimpleMapper.Map(model, new AddField());

            command.Properties = command.Properties ?? new JObject();

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/")]
        public Task Update(string name, long fieldId, [FromBody] UpdateFieldDto model)
        {
            var command = new UpdateField { FieldId = fieldId, Properties = model.Properties };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/hide/")]
        public Task Hide(string name, long fieldId)
        {
            var command = new HideField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/show/")]
        public Task Show(string name, long fieldId)
        {
            var command = new ShowField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/enable/")]
        public Task Enable(string name, long fieldId)
        {
            var command = new EnableField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("api/schemas/{name}/fields/{fieldId:long}/disable/")]
        public Task Disable(string name, long fieldId)
        {
            var command = new DisableField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpDelete]
        [Route("api/schemas/{name}/fields/{fieldId:long}/")]
        public Task Delete(string name, long fieldId)
        {
            var command = new DeleteField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
    }
}