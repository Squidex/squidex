﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Schemas))]
    public sealed class SchemaFieldsController : ApiController
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
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostField(string app, string name, [FromBody] AddFieldDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(app, command);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Add a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="request">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 201 => Schema field created.
        /// 400 => Schema field properties not valid.
        /// 409 => Schema field name already in use.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostNestedField(string app, string name, long parentId, [FromBody] AddFieldDto request)
        {
            var command = request.ToCommand(parentId);

            var response = await InvokeCommandAsync(app, command);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Reorders the fields.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The request that contains the field ids.</param>
        /// <returns>
        /// 200 => Schema fields reordered.
        /// 400 => Schema field ids do not cover the fields of the schema.
        /// 404 => Schema or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/ordering/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutSchemaFieldOrdering(string app, string name, [FromBody] ReorderFieldsDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Reorders the nested fields.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="request">The request that contains the field ids.</param>
        /// <returns>
        /// 200 => Schema fields reordered.
        /// 400 => Schema field ids do not cover the fields of the schema.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/ordering/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutNestedFieldOrdering(string app, string name, long parentId, [FromBody] ReorderFieldsDto request)
        {
            var command = request.ToCommand(parentId);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Update a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to update.</param>
        /// <param name="request">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 200 => Schema field updated.
        /// 400 => Schema field properties not valid or field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutField(string app, string name, long id, [FromBody] UpdateFieldDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Update a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to update.</param>
        /// <param name="request">The field object that needs to be added to the schema.</param>
        /// <returns>
        /// 200 => Schema field updated.
        /// 400 => Schema field properties not valid or field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutNestedField(string app, string name, long parentId, long id, [FromBody] UpdateFieldDto request)
        {
            var command = request.ToCommand(id, parentId);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Lock a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to lock.</param>
        /// <returns>
        /// 200 => Schema field shown.
        /// 400 => Schema field already locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A locked field cannot be updated or deleted.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/lock/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> LockField(string app, string name, long id)
        {
            var command = new LockField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Lock a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to lock.</param>
        /// <returns>
        /// 200 => Schema field hidden.
        /// 400 => Schema field already hidden.
        /// 404 => Field, schema, or app not found.
        /// </returns>
        /// <remarks>
        /// A locked field cannot be edited or deleted.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/lock/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> LockNestedField(string app, string name, long parentId, long id)
        {
            var command = new LockField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Hide a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to hide.</param>
        /// <returns>
        /// 200 => Schema field hidden.
        /// 400 => Schema field already hidden.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/hide/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> HideField(string app, string name, long id)
        {
            var command = new HideField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Hide a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to hide.</param>
        /// <returns>
        /// 200 => Schema field hidden.
        /// 400 => Schema field already hidden.
        /// 404 => Field, schema, or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/hide/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> HideNestedField(string app, string name, long parentId, long id)
        {
            var command = new HideField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Show a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to show.</param>
        /// <returns>
        /// 200 => Schema field shown.
        /// 400 => Schema field already visible.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/show/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> ShowField(string app, string name, long id)
        {
            var command = new ShowField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Show a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to show.</param>
        /// <returns>
        /// 200 => Schema field shown.
        /// 400 => Schema field already visible.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A hidden field is not part of the API response, but can still be edited in the portal.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/show/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> ShowNestedField(string app, string name, long parentId, long id)
        {
            var command = new ShowField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Enable a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to enable.</param>
        /// <returns>
        /// 200 => Schema field enabled.
        /// 400 => Schema field already enabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore, but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/enable/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableField(string app, string name, long id)
        {
            var command = new EnableField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Enable a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to enable.</param>
        /// <returns>
        /// 200 => Schema field enabled.
        /// 400 => Schema field already enabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore, but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/enable/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableNestedField(string app, string name, long parentId, long id)
        {
            var command = new EnableField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Disable a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 200 => Schema field disabled.
        /// 400 => Schema field already disabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore, but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/disable/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableField(string app, string name, long id)
        {
            var command = new DisableField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Disable nested a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 200 => Schema field disabled.
        /// 400 => Schema field already disabled.
        /// 404 => Schema, field or app not found.
        /// </returns>
        /// <remarks>
        /// A disabled field cannot not be edited in the squidex portal anymore, but will be part of the API response.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/disable/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableNestedField(string app, string name, long parentId, long id)
        {
            var command = new DisableField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 200 => Schema field deleted.
        /// 400 => Field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/fields/{id:long}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteField(string app, string name, long id)
        {
            var command = new DeleteField { FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a nested schema field.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="parentId">The parent field id.</param>
        /// <param name="id">The id of the field to disable.</param>
        /// <returns>
        /// 200 => Schema field deleted.
        /// 400 => Field is locked.
        /// 404 => Schema, field or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/fields/{parentId:long}/nested/{id:long}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto), 200)]
        [ApiPermission(Permissions.AppSchemasUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteNestedField(string app, string name, long parentId, long id)
        {
            var command = new DeleteField { ParentFieldId = parentId, FieldId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        private async Task<SchemaDetailsDto> InvokeCommandAsync(string app, ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ISchemaEntity>();
            var response = SchemaDetailsDto.FromSchemaWithDetails(result, this, app);

            return response;
        }
    }
}