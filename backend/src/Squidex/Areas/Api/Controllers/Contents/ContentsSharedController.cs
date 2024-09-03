// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;
using Squidex.Web.GraphQL;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents;

[SchemaMustBePublished]
[ApiExplorerSettings(GroupName = nameof(Contents))]
public sealed class ContentsSharedController : ApiController
{
    private static readonly GraphQLHttpMiddlewareOptions GraphQLOptions = new GraphQLHttpMiddlewareOptions
    {
        DefaultResponseContentType = new MediaTypeHeaderValue("application/json"),
        CsrfProtectionEnabled = false,
        CsrfProtectionHeaders = []
    };

    private readonly IContentQueryService contentQuery;
    private readonly IContentWorkflow contentWorkflow;

    public ContentsSharedController(ICommandBus commandBus,
        IContentQueryService contentQuery,
        IContentWorkflow contentWorkflow)
        : base(commandBus)
    {
        this.contentQuery = contentQuery;
        this.contentWorkflow = contentWorkflow;
    }

    /// <summary>
    /// GraphQL endpoint.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="200">Contents returned or mutated.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [Route("content/{app}/graphql/")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(2)]
    [AcceptHeader.Unpublished]
    [IgnoreCacheFilter]
    public IActionResult GetGraphQL(string app, GraphQLQueryDto request)
    {
        return new GraphQLExecutionActionResult<DummySchema>(GraphQLOptions);
    }

    /// <summary>
    /// GraphQL endpoint.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Contents returned or mutated.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost("content/{app}/graphql/")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(2)]
    [AcceptAnyBody]
    [AcceptHeader.Unpublished]
    [IgnoreCacheFilter]
    public IActionResult PostGraphQL(string app)
    {
        return new GraphQLExecutionActionResult<DummySchema>(GraphQLOptions);
    }

    /// <summary>
    /// GraphQL batch endpoint.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The request object.</param>
    /// <response code="200">Contents returned or mutated.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpGet("content/{app}/graphql/batch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(2)]
    [AcceptHeader.Unpublished]
    [IgnoreCacheFilter]
    public IActionResult GetGraphQLBatch(string app, GraphQLQueryDto request)
    {
        return new GraphQLExecutionActionResult<DummySchema>(GraphQLOptions);
    }

    /// <summary>
    /// GraphQL batch endpoint.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Contents returned or mutated.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost("content/{app}/graphql/batch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(2)]
    [AcceptAnyBody]
    [AcceptHeader.Unpublished]
    [IgnoreCacheFilter]
    public IActionResult PostGraphQLBatch(string app)
    {
        return new GraphQLExecutionActionResult<DummySchema>(GraphQLOptions);
    }

    /// <summary>
    /// Queries contents.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="query">The query object.</param>
    /// <response code="200">Contents returned.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpGet]
    [Route("content/{app}/")]
    [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [AcceptHeader.Fields]
    [AcceptHeader.Flatten]
    [AcceptHeader.Languages]
    [AcceptHeader.NoSlowTotal]
    [AcceptHeader.NoTotal]
    [AcceptHeader.Unpublished]
    public async Task<IActionResult> GetAllContents(string app, AllContentsByGetDto query)
    {
        var contents = await contentQuery.QueryAsync(Context, (query ?? new AllContentsByGetDto()).ToQuery(Request), HttpContext.RequestAborted);

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
    /// <response code="200">Contents returned.</response>
    /// <response code="404">App not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost]
    [Route("content/{app}/")]
    [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [AcceptHeader.Fields]
    [AcceptHeader.Flatten]
    [AcceptHeader.Languages]
    [AcceptHeader.NoSlowTotal]
    [AcceptHeader.NoTotal]
    [AcceptHeader.Unpublished]
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
    /// <response code="201">Contents created, update or delete.</response>
    /// <response code="400">Contents request not valid.</response>
    /// <response code="404">Contents references, schema or app not found.</response>
    /// <remarks>
    /// You can read the generated documentation for your app at /api/content/{appName}/docs.
    /// </remarks>
    [HttpPost]
    [Route("content/{app}/bulk")]
    [ProducesResponseType(typeof(BulkResultDto[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppContentsReadOwn)]
    [ApiCosts(5)]
    public async Task<IActionResult> BulkUpdateAllContents(string app, string schema, [FromBody] BulkUpdateContentsDto request)
    {
        var command = request.ToCommand(true);

        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<BulkUpdateResult>();
        var response = result.Select(x => BulkResultDto.FromDomain(x, HttpContext)).ToArray();

        return Ok(response);
    }
}
