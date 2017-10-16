// ==========================================================================
//  SchemaFieldsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Schemas.Models;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [MustBeAppDeveloper]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Schemas))]
    public sealed class SchemaFieldsController : ControllerBase
    {
        public SchemaFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Add a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 201 => Schema field created.
        /// 400 => Schema field properties not valid.
        /// 404 => Schema or app not found.
        /// 409 => Schema field name already in use.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/fields/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostField(string app, string name, [FromBody] AddFieldDto request)
        {
            var command = new AddField
            {
                Name = request.Name,
                Partitioning = request.Partitioning,
                Properties = request.Properties.ToProperties()
            };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<long>>();
            var response = new EntityCreatedDto { Id = result.IdOrValue.ToString(), Version = result.Version };

            return StatusCode(201, response);
        }

        /// <summary>
        /// Reorders the fields.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The request that contains the field ids.</param>
        /// <returns>
        /// 204 => Schema fields reorderd.
        /// 400 => Schema field ids do not cover the fields of the schema.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/ordering/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutFieldOrdering(string app, string name, [FromBody] ReorderFields request)
        {
            var command = new ReorderFields { FieldIds = request.FieldIds };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Update a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to update.</param>
        /// <param name="request">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 204 => Schema field updated.
        /// 400 => Schema field properties not valid or field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutField(string app, string name, long id, [FromBody] UpdateFieldDto request)
        {
            var command = new UpdateField { FieldId = id, Properties = request.Properties.ToProperties() };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Lock a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to lock.</param>
        /// <returns>
        /// 204 => Schema field shown.
        /// 400 => Schema field already locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/lock/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> LockField(string app, string name, long id)
        {
            var command = new LockField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Hide a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to hide.</param>
        /// <returns>
        /// 204 => Schema field hidden.
        /// 400 => Schema field already hidden.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A locked field cannot be edited or deleted.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/hide/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> HideField(string app, string name, long id)
        {
            var command = new HideField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Show a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to show.</param>
        /// <returns>
        /// 204 => Schema field shown.
        /// 400 => Schema field already visible.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/show/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> ShowField(string app, string name, long id)
        {
            var command = new ShowField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Enable a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to enable.</param>
        /// <returns>
        /// 204 => Schema field enabled.
        /// 400 => Schema field already enabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore,
        /// but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/enable/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableField(string app, string name, long id)
        {
            var command = new EnableField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Disable a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 204 => Schema field disabled.
        /// 400 => Schema field already disabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore,
        /// but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/disable/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableField(string app, string name, long id)
        {
            var command = new DisableField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 204 => Schema field deleted.
        /// 400 => Field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteField(string app, string name, long id)
        {
            var command = new DeleteField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}