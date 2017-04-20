// ==========================================================================
//  SchemasController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Controllers.Api.Schemas.Models;
using Squidex.Controllers.Api.Schemas.Models.Converters;
using Squidex.Core.Identity;
using Squidex.Core.Schemas;
using Squidex.Pipeline;
using Squidex.Read.Schemas.Repositories;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Controllers.Api.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [Authorize(Roles = SquidexRoles.AppDeveloper)]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Schemas")]
    public class SchemasController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        
        public SchemasController(ICommandBus commandBus, ISchemaRepository schemaRepository)
            : base(commandBus)
        {
            this.schemaRepository = schemaRepository;
        }

        /// <summary>
        /// Get schemas.
        /// </summary>
        /// <param name="app">The name of the app to create the schema to.</param>
        /// <returns>
        /// 200 => Schemas returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(SchemaDto[]), 200)]
        public async Task<IActionResult> GetSchemas(string app)
        {
            var schemas = await schemaRepository.QueryAllAsync(AppId);

            var model = schemas.Select(s => SimpleMapper.Map(s, new SchemaDto())).ToList();

            return Ok(model);
        }

        /// <summary>
        /// Get a schema by name.
        /// </summary>
        /// <param name="name">The name of the schema to retrieve.</param>
        /// <param name="app">The name of the app to create the schema to.</param>
        /// <returns>
        /// 200 => Schema found.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/schemas/{name}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto[]), 200)]
        public async Task<IActionResult> GetSchema(string app, string name)
        {
            var entity = await schemaRepository.FindSchemaAsync(AppId, name);

            if (entity == null)
            {
                return NotFound();
            }

            var model = entity.ToModel();

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(model);
        }

        /// <summary>
        /// Create a new schema.
        /// </summary>
        /// <param name="request">The schema object that needs to be added to the app.</param>
        /// <param name="app">The name of the app to create the schema to.</param>
        /// <returns>
        /// 201 => Schema created.  
        /// 400 => Schema name or properties are not valid.
        /// 409 => Schema name already in use.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        public async Task<IActionResult> PostSchema(string app, [FromBody] CreateSchemaDto request)
        {
            var command = SimpleMapper.Map(request, new CreateSchema());

            await CommandBus.PublishAsync(command);

            return CreatedAtAction(nameof(GetSchema), new { name = request.Name }, new EntityCreatedDto { Id = command.Name });
        }

        /// <summary>
        /// Update a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema has been updated.
        /// 400 => Schema properties are not valid.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/")]
        public async Task<IActionResult> PutSchema(string app, string name, [FromBody] UpdateSchemaDto request)
        {
            var properties = SimpleMapper.Map(request, new SchemaProperties());

            await CommandBus.PublishAsync(new UpdateSchema { Properties = properties });

            return NoContent();
        }

        /// <summary>
        /// Publish a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema to publish.</param>
        /// <returns>
        /// 204 => Schema has been published.
        /// 400 => Schema is already published.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/publish")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PublishSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new PublishSchema());

            return NoContent();
        }

        /// <summary>
        /// Unpublish a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema to unpublish.</param>
        /// <returns>
        /// 204 => Schema has been unpublished.
        /// 400 => Schema is not published.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/unpublish")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> UnpublishSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new UnpublishSchema());

            return NoContent();
        }

        /// <summary>
        /// Delete a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema to delete.</param>
        /// <returns>
        /// 204 => Schema has been deleted.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/")]
        public async Task<IActionResult> DeleteSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}