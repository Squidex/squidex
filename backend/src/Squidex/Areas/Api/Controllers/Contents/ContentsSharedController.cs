// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;
using Squidex.Web.GraphQL;

namespace Squidex.Areas.Api.Controllers.Contents;

[SchemaMustBePublished]
public sealed class ContentsSharedController : ApiController
{
    private readonly IContentQueryService contentQuery;
    private readonly IContentWorkflow contentWorkflow;
    private readonly GraphQLRunner graphQLRunner;

    public ContentsSharedController(ICommandBus commandBus,
        IContentQueryService contentQuery,
        IContentWorkflow contentWorkflow,
        GraphQLRunner graphQLRunner)
        : base(commandBus)
    {
        this.contentQuery = contentQuery;
        this.contentWorkflow = contentWorkflow;
        this.graphQLRunner = graphQLRunner;
    }

    /// <summary>
    /// GraphQL endpoint.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Contents returned or mutated.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [Route("content/{app}/graphql/")]
    [Route("content/{app}/graphql/batch")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(2)]
    public Task GetGraphQL(string app)
    {
        return graphQLRunner.InvokeAsync(HttpContext);
    }

    /// <summary>
    /// Queries contents.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="query">The required query object.</param>
    /// <response code="200">Contents returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpGet]
    [Route("content/{app}/")]
    [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAllContents(string app, AllContentsByGetDto query)
    {
        var contents = await contentQuery.QueryAsync(Context, query?.ToQuery() ?? Q.Empty, HttpContext.RequestAborted);

        var response = Deferred.AsyncResponse(() =>
        {
            return ContentsDto.FromContentsAsync(contents, Resources, null, contentWorkflow);
        });

        return Ok(response);
    }

    /// <summary>
    /// Queries contents.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="query">The required query object.</param>
    /// <response code="200">Contents returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost]
    [Route("content/{app}/")]
    [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAllContentsPost(string app, [FromBody] AllContentsByPostDto query)
    {
        var contents = await contentQuery.QueryAsync(Context, query?.ToQuery() ?? Q.Empty, HttpContext.RequestAborted);

        var response = Deferred.AsyncResponse(() =>
        {
            return ContentsDto.FromContentsAsync(contents, Resources, null, contentWorkflow);
        });

        return Ok(response);
    }

    /// <summary>
    /// Bulk update content items.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="schema">The name of the schema.</param>
    /// <param name="request">The bulk update request.</param>
    /// <response code="201">Contents created, update or delete.</response>.
    /// <response code="400">Contents request not valid.</response>.
    /// <response code="404">Contents references, schema or app not found.</response>.
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost]
    [Route("content/{app}/bulk")]
    [ProducesResponseType(typeof(BulkResultDto[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppContentsReadOwn)]
    [ApiCosts(5)]
    public async Task<IActionResult> BulkUpdateContents(string app, string schema, [FromBody] BulkUpdateContentsDto request)
    {
        var command = request.ToCommand(true);

        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<BulkUpdateResult>();
        var response = result.Select(x => BulkResultDto.FromDomain(x, HttpContext)).ToArray();

        return Ok(response);
    }
}
