﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.GenerateFilters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas;

/// <summary>
/// Update and query information about schemas.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Schemas))]
public sealed class SchemasController(
    ICommandBus commandBus,
    IContentWorkflow workflow,
    IAppProvider appProvider,
    SchemaAIGenerator schemaAIGenerator,
    ScriptingCompleter scriptingCompleter)
    : ApiController(commandBus)
{
    /// <summary>
    /// Get schemas.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Schemas returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/schemas/")]
    [ProducesResponseType(typeof(SchemasDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetSchemas(string app)
    {
        var schemas = await appProvider.GetSchemasAsync(AppId, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return SchemasDto.FromDomain(schemas, Resources);
        });

        Response.Headers[HeaderNames.ETag] = schemas.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Get a schema by name.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema to retrieve.</param>
    /// <response code="200">Schema found.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasRead)]
    [ApiCosts(0)]
    public IActionResult GetSchema(string app, string schema)
    {
        var response = Deferred.Response(() =>
        {
            return SchemaDto.FromDomain(Schema, Resources);
        });

        Response.Headers[HeaderNames.ETag] = Schema.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Create a new schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The schema object that needs to be added to the app.</param>
    /// <response code="201">Schema created.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="409">Schema name already in use.</response>
    [HttpPost]
    [Route("apps/{app}/schemas/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostSchema(string app, [FromBody] CreateSchemaDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetSchema), new { app, schema = request.Name }, response);
    }

    /// <summary>
    /// Generate a new schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The schema object that needs to be added to the app.</param>
    /// <response code="201">Schema created.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="409">Schema name already in use.</response>
    [HttpPost]
    [Route("apps/{app}/schemas/generate")]
    [ProducesResponseType(typeof(GenerateSchemaResponseDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostSchemaGenerate(string app, [FromBody] GenerateSchemaDto request)
    {
        var result =
            await schemaAIGenerator.ExecuteAsync(
                App,
                request.Prompt,
                request.NumberOfContentItems,
                request.Execute,
                HttpContext.RequestAborted);

        var response = GenerateSchemaResponseDto.FromDomain(result);

        return Ok(response);
    }

    /// <summary>
    /// Update a schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The schema object that needs to updated.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutSchema(string app, string schema, [FromBody] UpdateSchemaDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Synchronize a schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The schema object that needs to updated.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/sync")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutSchemaSync(string app, string schema, [FromBody] SynchronizeSchemaDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Update a schema category.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The schema object that needs to updated.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/category")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutCategory(string app, string schema, [FromBody] ChangeCategoryDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Update the preview urls.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The preview urls for the schema.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/preview-urls")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutPreviewUrls(string app, string schema, [FromBody] ConfigurePreviewUrlsDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Update the scripts.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The schema scripts object that needs to updated.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/scripts/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasScripts)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutScripts(string app, string schema, [FromBody] SchemaScriptsDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Update the rules.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The schema rules object that needs to updated.</param>
    /// <response code="200">Schema updated.</response>
    /// <response code="400">Schema request not valid.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/rules/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutRules(string app, string schema, [FromBody] ConfigureFieldRulesDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Publish a schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema to publish.</param>
    /// <response code="200">Schema published.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/publish/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasPublish)]
    [ApiCosts(1)]
    public async Task<IActionResult> PublishSchema(string app, string schema)
    {
        var command = new PublishSchema();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Unpublish a schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema to unpublish.</param>
    /// <response code="200">Schema unpublished.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/schemas/{schema}/unpublish/")]
    [ProducesResponseType(typeof(SchemaDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasPublish)]
    [ApiCosts(1)]
    public async Task<IActionResult> UnpublishSchema(string app, string schema)
    {
        var command = new UnpublishSchema();

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Delete a schema.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema to delete.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="204">Schema deleted.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpDelete]
    [Route("apps/{app}/schemas/{schema}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteSchema(string app, string schema, DeleteSchemaDto request)
    {
        var command = request.ToCommand();

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/completion/content-scripts")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetContentScriptsCompletion(string app, string schema)
    {
        var completion = scriptingCompleter.ContentScript(await BuildModel());

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/completion/content-triggers")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetContentTriggersCompletion(string app, string schema)
    {
        var completion = scriptingCompleter.ContentTrigger(await BuildModel());

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/completion/field-rules")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetFieldRulesCompletion(string app, string schema)
    {
        var completion = scriptingCompleter.FieldRule(await BuildModel());

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/completion/prview-urls")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPreviewUrlsCompletion(string app, string schema)
    {
        var completion = scriptingCompleter.PreviewUrl(await BuildModel());

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/filters")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetFilters(string app, string schema)
    {
        var components = await appProvider.GetComponentsAsync(Schema, HttpContext.RequestAborted);

        var result = ContentQueryModel.Build(Schema, App.PartitionResolver(), components).Flatten();
        var response = await QueryModelDto.FromModelAsync(result, Schema, workflow);

        return Ok(response);
    }

    private async Task<FilterSchema> BuildModel()
    {
        var components = await appProvider.GetComponentsAsync(Schema, HttpContext.RequestAborted);

        return Schema.BuildDataSchema(App.PartitionResolver(), components);
    }

    private Task<Schema?> GetSchemaAsync(string schema)
    {
        if (Guid.TryParseExact(schema, "D", out var guid))
        {
            var schemaId = DomainId.Create(guid);

            return appProvider.GetSchemaAsync(AppId, schemaId, ct: HttpContext.RequestAborted);
        }
        else
        {
            return appProvider.GetSchemaAsync(AppId, schema, ct: HttpContext.RequestAborted);
        }
    }

    private async Task<SchemaDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<Schema>();
        var response = SchemaDto.FromDomain(result, Resources);

        return response;
    }
}
