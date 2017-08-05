// ==========================================================================
//  SchemasController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Schemas.Models;
using Squidex.Controllers.Api.Schemas.Models.Converters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
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
        /// <param name="app">The name of the app to get the schemas for.</param>
        /// <returns>
        /// 200 => Schemas returned.
        /// 404 => App not found.
        /// </returns>
        [MustBeAppEditor]
        [HttpGet]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(SchemaDto[]), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSchemas(string app)
        {
            var schemas = await schemaRepository.QueryAllAsync(AppId);

            var response = schemas.Select(s => s.ToModel()).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get a schema by name.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema to retrieve.</param>
        /// <returns>
        /// 200 => Schema found.
        /// 404 => Schema or app not found.
        /// </returns>
        [MustBeAppEditor]
        [HttpGet]
        [Route("apps/{app}/schemas/{name}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto[]), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSchema(string app, string name)
        {
            ISchemaEntity entity;

            if (Guid.TryParse(name, out var id))
            {
                entity = await schemaRepository.FindSchemaAsync(id);
            }
            else
            {
                entity = await schemaRepository.FindSchemaAsync(AppId, name);
            }

            if (entity == null)
            {
                return NotFound();
            }

            var response = entity.ToDetailsModel();

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Create a new schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The schema object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Schema created.
        /// 400 => Schema name or properties are not valid.
        /// 409 => Schema name already in use.
        /// </returns>
        [MustBeAppDeveloper]
        [HttpPost]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostSchema(string app, [FromBody] CreateSchemaDto request)
        {
            var command = request.ToCommand();

            await CommandBus.PublishAsync(command);

            return CreatedAtAction(nameof(GetSchema), new { name = request.Name }, new EntityCreatedDto { Id = command.Name });
        }

        /// <summary>
        /// Update a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema has been updated.
        /// 400 => Schema properties are not valid.
        /// 404 => Schema or app not found.
        /// </returns>
        [MustBeAppDeveloper]
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutSchema(string app, string name, [FromBody] UpdateSchemaDto request)
        {
            var properties = SimpleMapper.Map(request, new SchemaProperties());

            await CommandBus.PublishAsync(new UpdateSchema { Properties = properties });

            return NoContent();
        }

        /// <summary>
        /// Publish a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema to publish.</param>
        /// <returns>
        /// 204 => Schema has been published.
        /// 400 => Schema is already published.
        /// 404 => Schema or app not found.
        /// </returns>
        [MustBeAppDeveloper]
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/publish")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new PublishSchema());

            return NoContent();
        }

        /// <summary>
        /// Unpublish a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema to unpublish.</param>
        /// <returns>
        /// 204 => Schema has been unpublished.
        /// 400 => Schema is not published.
        /// 404 => Schema or app not found.
        /// </returns>
        [MustBeAppDeveloper]
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/unpublish")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new UnpublishSchema());

            return NoContent();
        }

        /// <summary>
        /// Delete a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema to delete.</param>
        /// <returns>
        /// 204 => Schema has been deleted.
        /// 404 => Schema or app not found.
        /// </returns>
        [MustBeAppDeveloper]
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}