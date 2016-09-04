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
using Swashbuckle.SwaggerGen.Annotations;

#pragma warning disable 1584,1711,1572,1573,1581,1580

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasFieldsController : BaseController
    {
        public SchemasFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Adds a new field to the schema with the specified name.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="command">The field properties</param>
        [HttpPost]
        [Route("schemas/{name}/fields/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Add(string name, [FromBody] ModelFieldProperties field)
        {
            var command = new AddModelField { Properties = field };

            return CommandBus.PublishAsync(command);
        }

        /// <summary>
        /// Uüdates the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        /// <param name="command">The field properties</param>
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Update(string name, long fieldId, [FromBody] UpdateModelField command)
        {
            return CommandBus.PublishAsync(command);
        }

        /// <summary>
        /// Hides the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/hide/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Hide(string name, long fieldId, HideModelField command)
        {
            return CommandBus.PublishAsync(command);
        }

        /// <summary>
        /// Sows the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/show/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Show(string name, long fieldId, ShowModelField command)
        {
            return CommandBus.PublishAsync(command);
        }

        /// <summary>
        /// Enables the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/enable/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Enable(string name, long fieldId, EnableModelField command)
        {
            return CommandBus.PublishAsync(command);
        }

        /// <summary>
        /// Disables the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        [HttpPut]
        [Route("schemas/{name}/fields/{fieldId:long}/disable/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Disable(string name, long fieldId, DisableModelField command)
        {
            return CommandBus.PublishAsync(command);
        }


        /// <summary>
        /// Deletes the field with the specified schema name and field id.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="fieldId">The id of the field.</param>
        [HttpDelete]
        [Route("schemas/{name}/fields/{fieldId:long}/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task Delete(string name, long fieldId, DeleteModelField command)
        {
            return CommandBus.PublishAsync(command);
        }
    }
}