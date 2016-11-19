// ==========================================================================
//  SchemaFieldsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Schemas.Models;
using Squidex.Pipeline;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Modules.Api.Schemas
{
    [Authorize]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    public class SchemasFieldsController : ControllerBase
    {
        public SchemasFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }
        
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/fields/")]
        public Task Add(string name, [FromBody] CreateFieldDto model)
        {
            var command = SimpleMapper.Map(model, new AddField());

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{fieldId:long}/")]
        public Task Update(string name, long fieldId, [FromBody] UpdateFieldDto model)
        {
            var command = SimpleMapper.Map(model, new UpdateField());

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{fieldId:long}/hide/")]
        public Task Hide(string name, long fieldId)
        {
            var command = new HideField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{fieldId:long}/show/")]
        public Task Show(string name, long fieldId)
        {
            var command = new ShowField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/enable/")]
        public Task Enable(string name, long fieldId)
        {
            var command = new EnableField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
        
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{fieldId:long}/disable/")]
        public Task Disable(string name, long fieldId)
        {
            var command = new DisableField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }

        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/fields/{fieldId:long}/")]
        public Task Delete(string name, long fieldId)
        {
            var command = new DeleteField { FieldId = fieldId };

            return CommandBus.PublishAsync(command);
        }
    }
}