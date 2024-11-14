// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Indexes;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas;

/// <summary>
/// Update and query information about schemas.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Schemas))]
[ApiModelValidation(true)]
public class SchemaIndexesController : ApiController
{
    private readonly ICommandBus commandBus;
    private readonly IJobService jobService;
    private readonly IContentRepository contentRepository;

    public SchemaIndexesController(ICommandBus commandBus, IJobService jobService, IContentRepository contentRepository)
        : base(commandBus)
    {
        this.commandBus = commandBus;
        this.jobService = jobService;
        this.contentRepository = contentRepository;
    }

    /// <summary>
    /// Gets the schema indexes.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <response code="200">Schema indexes returned.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpGet]
    [Route("apps/{app}/schemas/{schema}/indexes/")]
    [ProducesResponseType(typeof(IndexesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasIndexes)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetIndexes(string app, string schema)
    {
        var indexes = await contentRepository.GetIndexesAsync(App.Id, Schema.Id, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return IndexesDto.FromDomain(indexes, Resources);
        });

        return Ok(response);
    }

    /// <summary>
    /// Create a schema indexes.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The request object that represents an index.</param>
    /// <response code="200">Schema findexes returned.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPost]
    [Route("apps/{app}/schemas/{schema}/indexes/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasIndexes)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostIndex(string app, string schema, [FromBody] CreateIndexDto request)
    {
        var job = CreateIndexJob.BuildRequest(User.Token()!, App, Schema, request.ToIndex());

        await jobService.StartAsync(App.Id, job, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Create a schema indexes.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="name">The name of the index.</param>
    /// <response code="204">Schema index deletion added to job queue.</response>
    /// <response code="404">Schema or app not found.</response>
    [HttpPost]
    [Route("apps/{app}/schemas/{schema}/indexes/{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppSchemasIndexes)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteIndex(string app, string schema, string name)
    {
        var job = DropIndexJob.BuildRequest(User.Token()!, App, Schema, name);

        await jobService.StartAsync(App.Id, job, HttpContext.RequestAborted);

        return NoContent();
    }
}
