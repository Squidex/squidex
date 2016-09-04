// ==========================================================================
//  SchemasController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Core.Schema;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Read.Models;
using PinkParrot.Write.Schema.Commands;
using Swashbuckle.SwaggerGen.Annotations;

#pragma warning disable 1584,1711,1572,1581,1580

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasController : Controller
    {
        private readonly ICommandBus commandBus;

        public SchemasController(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        /// <summary>
        /// Queries all your schemas.
        /// </summary>
        [HttpGet]
        [Route("schemas/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public Task<List<ModelSchemaRM>> Query()
        {
            return null;
        }

        /// <summary>
        /// Creates a new schema.
        /// </summary>
        /// <param name="schema">The properties of the schema.</param>
        /// <remarks>
        /// Field can be managed later.
        /// </remarks>
        /// <response code="201">Schema created</response>
        /// <response code="500">Schema update failed</response>
        [HttpPost]
        [Route("schemas/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        [ProducesResponseType(typeof(EntityCreated), 204)]
        public async Task<ActionResult> Create([FromBody] ModelSchemaProperties schema)
        {
            var command = new CreateModelSchema { AggregateId = Guid.NewGuid(), Properties = schema };

            await commandBus.PublishAsync(command);

            return CreatedAtAction("Query", new EntityCreated { Id = command.AggregateId });
        }

        /// <summary>
        /// Updates the schema with the specified name.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="schema">The properties of the schema.</param>
        /// <response code="204">Schema update</response>
        /// <response code="404">Schema not found</response>
        /// <response code="500">Schema update failed</response>
        [HttpPut]
        [Route("schemas/{name}/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public async Task<ActionResult> Update(string name, [FromBody] ModelSchemaProperties schema)
        {
            var command = new UpdateModelSchema { Properties = schema };

            await commandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Deletes the schema with the specified name.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <response code="204">Schema deleted</response>
        /// <response code="404">Schema not found</response>
        /// <response code="500">Schema deletion failed</response>
        [HttpDelete]
        [Route("schemas/{name}/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        public async Task<ActionResult> Delete(string name)
        {
            await commandBus.PublishAsync(new DeleteModelSchema());

            return NoContent();
        }
    }
}