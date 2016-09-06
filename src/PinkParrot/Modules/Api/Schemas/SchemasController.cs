// ==========================================================================
//  SchemasController.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PinkParrot.Core.Schema;
using PinkParrot.Core.Schema.Json;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Read.Repositories;
using PinkParrot.Write.Schema.Commands;
using Swashbuckle.SwaggerGen.Annotations;

#pragma warning disable 1584,1711,1572,1581,1580

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasController : ControllerBase
    {
        private readonly IModelSchemaRepository modelSchemaRepository;
        
        public SchemasController(ICommandBus commandBus, IModelSchemaRepository modelSchemaRepository)
            : base(commandBus)
        {
            this.modelSchemaRepository = modelSchemaRepository;
        }

        /// <summary>
        /// Queries all your schemas.
        /// </summary>
        [HttpGet]
        [Route("schemas/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        [ProducesResponseType(typeof(List<SchemasDto>), 200)]
        public async Task<List<SchemasDto>> Query()
        {
            var schemas = await modelSchemaRepository.QueryAllAsync(TenantId);

            return schemas.Select(s => SimpleMapper.Map(s, new SchemasDto())).ToList();
        }

        /// <summary>
        /// Gets the schema with the specified name.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <response code="200">Schema returned</response>
        /// <response code="404">Schema not found</response>
        [HttpGet]
        [Route("schemas/{name}/")]
        [SwaggerOperation(Tags = new[] { "Schemas" })]
        [ProducesResponseType(typeof(SchemaDto), 200)]
        public async Task<ActionResult> Get(string name)
        {
            var entity = await modelSchemaRepository.FindSchemaAsync(TenantId, name);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(SchemaDto.Create(entity.Schema));
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
        [ProducesResponseType(typeof(EntityCreated), 201)]
        public async Task<ActionResult> Create([FromBody] ModelSchemaProperties schema)
        {
            var command = new CreateModelSchema { AggregateId = Guid.NewGuid(), Properties = schema };

            await CommandBus.PublishAsync(command);

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
        [ProducesResponseType(typeof(void), 204)]
        public async Task<ActionResult> Update(string name, [FromBody] ModelSchemaProperties schema)
        {
            var command = new UpdateModelSchema { Properties = schema };

            await CommandBus.PublishAsync(command);

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
        [ProducesResponseType(typeof(void), 204)]
        public async Task<ActionResult> Delete(string name)
        {
            await CommandBus.PublishAsync(new DeleteModelSchema());

            return NoContent();
        }
    }
}