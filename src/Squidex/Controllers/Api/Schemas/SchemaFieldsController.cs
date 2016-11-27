// ==========================================================================
//  SchemaFieldsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Controllers.Api.Schemas.Models;
using Squidex.Pipeline;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Controllers.Api.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [Authorize(Roles = "app-owner,app-developer")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Schemas")]
    public class SchemaFieldsController : ControllerBase
    {
        public SchemaFieldsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Create a new schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="model">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 201 => Field created.
        /// 409 => Field name already in use.
        /// 404 => App or schema not found.
        /// 404 => Field properties not valid.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/fields/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostField(string app, string name, [FromBody] AddFieldDto model)
        {
            var command = new AddField { Name = model.Name, Properties = model.Properties.ToProperties() };

            var context = await CommandBus.PublishAsync(command);
            var result = context.Result<long>();

            return StatusCode(201, new EntityCreatedDto { Id = result.ToString() });
        }

        /// <summary>
        /// Update a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="model">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 204 => Field created.
        /// 409 => Field name already in use.
        /// 404 => App, schema or field not found.
        /// 404 => Field properties not valid.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PutField(string app, string name, long id, [FromBody] UpdateFieldDto model)
        {
            var command = new UpdateField { FieldId = id, Properties = model.Properties.ToProperties() };

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
        /// 400 => Field already hidden.
        /// 204 => Schema field hidden.
        /// 404 => App, schema or field not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/hide/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
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
        /// <param name="id">The id of the field to shows.</param>
        /// <returns>
        /// 400 => Field already visible.
        /// 204 => Schema field shown.
        /// 404 => App, schema or field not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/show/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
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
        /// 400 => Field already enabled.
        /// 204 => Schema field enabled.
        /// 404 => App, schema or field not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore,
        /// but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/enable/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
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
        /// 400 => Field already disabled.
        /// 204 => Schema field disabled.
        /// 404 => App, schema or field not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore,
        /// but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/disable/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
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
        /// 404 => App, schema or field not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        public async Task<IActionResult> DeleteField(string app, string name, long id)
        {
            var command = new DeleteField { FieldId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}