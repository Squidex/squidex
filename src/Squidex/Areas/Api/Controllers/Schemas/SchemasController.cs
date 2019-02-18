// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Schemas))]
    public sealed class SchemasController : ApiController
    {
        private readonly IAppProvider appProvider;

        public SchemasController(ICommandBus commandBus, IAppProvider appProvider)
            : base(commandBus)
        {
            this.appProvider = appProvider;
        }

        /// <summary>
        /// Get schemas.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Schemas returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(SchemaDto[]), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSchemas(string app)
        {
            var schemas = await appProvider.GetSchemasAsync(AppId);

            var response = schemas.ToArray(SchemaDto.FromSchema);

            Response.Headers[HeaderNames.ETag] = response.ToManyEtag();

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
        [HttpGet]
        [Route("apps/{app}/schemas/{name}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppCommon)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSchema(string app, string name)
        {
            ISchemaEntity entity;

            if (Guid.TryParse(name, out var id))
            {
                entity = await appProvider.GetSchemaAsync(AppId, id);
            }
            else
            {
                entity = await appProvider.GetSchemaAsync(AppId, name);
            }

            if (entity == null || entity.IsDeleted)
            {
                return NotFound();
            }

            var response = SchemaDetailsDto.FromSchema(entity);

            Response.Headers[HeaderNames.ETag] = entity.Version.ToString();

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
        [HttpPost]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ApiPermission(Permissions.AppSchemasCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostSchema(string app, [FromBody] CreateSchemaDto request)
        {
            var command = request.ToCommand();
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = new EntityCreatedDto { Id = command.SchemaId.ToString(), Version = result.Version };

            return CreatedAtAction(nameof(GetSchema), new { name = request.Name }, response);
        }

        /// <summary>
        /// Update a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// 400 => Schema properties are not valid.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/")]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutSchema(string app, string name, [FromBody] UpdateSchemaDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

            return NoContent();
        }

        /// <summary>
        /// Synchronize a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// 400 => Schema properties are not valid.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/sync")]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutSchemaSync(string app, string name, [FromBody] SynchronizeSchemaDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

            return NoContent();
        }

        /// <summary>
        /// Update a schema category.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/category")]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutCategory(string app, string name, [FromBody] ChangeCategoryDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

            return NoContent();
        }

        /// <summary>
        /// Update the preview urls.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The preview urls for the schema.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/preview-urls")]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutPreviewUrls(string app, string name, [FromBody] ConfigurePreviewUrlsDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

            return NoContent();
        }

        /// <summary>
        /// Update the scripts of a schema.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The schema scripts object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// 400 => Schema properties are not valid.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/scripts/")]
        [ApiPermission(Permissions.AppSchemasScripts)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutSchemaScripts(string app, string name, [FromBody] SchemaScriptsDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand());

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
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/publish/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiPermission(Permissions.AppSchemasPublish)]
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
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/unpublish/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiPermission(Permissions.AppSchemasPublish)]
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
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/")]
        [ApiPermission(Permissions.AppSchemasDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}