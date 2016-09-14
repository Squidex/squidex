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
using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Read.Models;
using PinkParrot.Read.Repositories;
using PinkParrot.Write.Schemas.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    public class SchemasController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        
        public SchemasController(ICommandBus commandBus, ISchemaRepository schemaRepository)
            : base(commandBus)
        {
            this.schemaRepository = schemaRepository;
        }

        [HttpGet]
        [Route("api/schemas/")]
        public async Task<List<ListSchemaDto>> Query()
        {
            var schemas = await schemaRepository.QueryAllAsync(TenantId);

            return schemas.Select(s => SimpleMapper.Map(s, new ListSchemaDto())).ToList();
        }

        [HttpGet]
        [Route("api/schemas/{name}/")]
        public async Task<ActionResult> Get(string name)
        {
            var entity = await schemaRepository.FindSchemaAsync(TenantId, name);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(SchemaDto.Create(entity.Schema));
        }

        [HttpPost]
        [Route("api/schemas/")]
        public async Task<ActionResult> Create([FromBody] CreateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new CreateSchema { AggregateId = Guid.NewGuid() });

            await CommandBus.PublishAsync(command);

            return CreatedAtAction("Query", new EntityCreatedDto { Id = command.AggregateId });
        }

        [HttpPut]
        [Route("api/schemas/{name}/")]
        public async Task<ActionResult> Update(string name, [FromBody] SchemaProperties schema)
        {
            var command = new UpdateSchema { Properties = schema };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpDelete]
        [Route("api/schemas/{name}/")]
        public async Task<ActionResult> Delete(string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}