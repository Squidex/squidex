// ==========================================================================
//  SchemasController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Schemas.Models;
using Squidex.Pipeline;
using Squidex.Read.Schemas.Repositories;
using Squidex.Store.MongoDb.Schemas.Models;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Modules.Api.Schemas
{
    [Authorize]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [Route("apps/{app}")]
    public class SchemasController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        
        public SchemasController(ICommandBus commandBus, ISchemaRepository schemaRepository)
            : base(commandBus)
        {
            this.schemaRepository = schemaRepository;
        }

        [HttpGet]
        [Route("schemas/")]
        public async Task<List<ListSchemaDto>> Query()
        {
            var schemas = await schemaRepository.QueryAllAsync(AppId);

            return schemas.Select(s => SimpleMapper.Map(s, new ListSchemaDto())).ToList();
        }

        [HttpGet]
        [Route("schemas/{name}/")]
        public async Task<ActionResult> Get(string name)
        {
            var entity = await schemaRepository.FindSchemaAsync(AppId, name);

            if (entity == null)
            {
                return NotFound();
            }

            var model = SchemaDto.Create(entity.Schema);

            return Ok(model);
        }

        [HttpPost]
        [Route("schemas/")]
        public async Task<ActionResult> Create([FromBody] CreateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new CreateSchema { AggregateId = Guid.NewGuid() });

            await CommandBus.PublishAsync(command);

            return CreatedAtAction("Get", new { name = model.Name }, new EntityCreatedDto { Id = command.Name });
        }

        [HttpPut]
        [Route("schemas/{name}/")]
        public async Task<ActionResult> Update(string name, [FromBody] UpdateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new UpdateSchema());

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpDelete]
        [Route("schemas/{name}/")]
        public async Task<ActionResult> Delete(string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}