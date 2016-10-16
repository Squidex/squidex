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
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Reflection;
using PinkParrot.Modules.Api.Schemas.Models;
using PinkParrot.Pipeline;
using PinkParrot.Read.Schemas.Repositories;
using PinkParrot.Store.MongoDb.Schemas.Models;
using PinkParrot.Write.Schemas.Commands;

namespace PinkParrot.Modules.Api.Schemas
{
    [ApiExceptionFilter]
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
            var schemas = await schemaRepository.QueryAllAsync(AppId);

            return schemas.Select(s => SimpleMapper.Map(s, new ListSchemaDto())).ToList();
        }

        [HttpGet]
        [Route("api/schemas/{name}/")]
        public async Task<ActionResult> Get(string name)
        {
            var entity = await schemaRepository.FindSchemaAsync(AppId, name);

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

            return CreatedAtAction("Get", new { name = model.Name }, new EntityCreatedDto { Id = command.Name });
        }

        [HttpPut]
        [Route("api/schemas/{name}/")]
        public async Task<ActionResult> Update(string name, [FromBody] UpdateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new UpdateSchema());

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