// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;
using Squidex.Web.GraphQL;

namespace Squidex.Areas.Api.Controllers.Contents
{
    [SchemaMustBePublishedAttribute]
    public sealed class ContentsController : ApiController
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContentWorkflow contentWorkflow;
        private readonly GraphQLMiddleware graphQLMiddleware;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IContentWorkflow contentWorkflow,
            GraphQLMiddleware graphQLMiddleware)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.contentWorkflow = contentWorkflow;

            this.graphQLMiddleware = graphQLMiddleware;
        }

        /// <summary>
        /// GraphQL endpoint.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Contents returned or mutated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [Route("content/{app}/graphql/batch")]
        [ApiPermissionOrAnonymous]
        [ApiCosts(2)]
        public Task GetGraphQL(string app)
        {
            return graphQLMiddleware.InvokeAsync(HttpContext);
        }

        /// <summary>
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="query">The required query object.</param>
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => App not found.
        /// </returns>
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
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => App not found.
        /// </returns>
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
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="ids">The optional ids of the content to fetch.</param>
        /// <param name="q">The optional json query.</param>
        /// <returns>
        /// 200 => Contents retunred.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContents(string app, string schema, [FromQuery] string? ids = null, [FromQuery] string? q = null)
        {
            var contents = await contentQuery.QueryAsync(Context, schema, CreateQuery(ids, q), HttpContext.RequestAborted);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, Schema, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="query">The required query object.</param>
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/query")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentsPost(string app, string schema, [FromBody] QueryDto query)
        {
            var contents = await contentQuery.QueryAsync(Context, schema, query?.ToQuery() ?? Q.Empty, HttpContext.RequestAborted);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, Schema, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <returns>
        /// 200 => Content returned.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/{id}/")]
        [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string app, string schema, DomainId id)
        {
            var content = await contentQuery.FindAsync(Context, schema, id, ct: HttpContext.RequestAborted);

            if (content == null)
            {
                return NotFound();
            }

            var response = ContentDto.FromContent(content, Resources);

            return Ok(response);
        }

        /// <summary>
        /// Get a content item validity.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <returns>
        /// 204 => Content is valid.
        /// 400 => Content not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/{id}/validity")]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentValidity(string app, string schema, DomainId id)
        {
            var command = new ValidateContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Get all references of a content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <param name="q">The optional json query.</param>
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/{id}/references")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetReferences(string app, string schema, DomainId id, [FromQuery] string? q = null)
        {
            var contents = await contentQuery.QueryAsync(Context, CreateQuery(null, q).WithReferencing(id), HttpContext.RequestAborted);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, null, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get a referencing contents of a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <param name="q">The optional json query.</param>
        /// <returns>
        /// 200 => Content returned.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/{id}/referencing")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetReferencing(string app, string schema, DomainId id, [FromQuery] string? q = null)
        {
            var contents = await contentQuery.QueryAsync(Context, CreateQuery(null, q).WithReference(id), HttpContext.RequestAborted);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, null, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get a content by version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <param name="version">The version fo the content to fetch.</param>
        /// <returns>
        /// 200 => Content version returned.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{schema}/{id}/{version}/")]
        [ApiPermissionOrAnonymous(Permissions.AppContentsReadOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string app, string schema, DomainId id, int version)
        {
            var content = await contentQuery.FindAsync(Context, schema, id, version, HttpContext.RequestAborted);

            if (content == null)
            {
                return NotFound();
            }

            var response = ContentDto.FromContent(content, Resources);

            return Ok(response.Data);
        }

        /// <summary>
        /// Create a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="request">The request parameters.</param>
        /// <returns>
        /// 201 => Content created.
        /// 400 => Content request not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/")]
        [ProducesResponseType(typeof(ContentsDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string schema, CreateContentDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetContent), new { app, schema, id = command.ContentId }, response);
        }

        /// <summary>
        /// Import content items.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="request">The import request.</param>
        /// <returns>
        /// 200 => Contents created.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/import")]
        [ProducesResponseType(typeof(BulkResultDto[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsCreate)]
        [ApiCosts(5)]
        [Obsolete("Use bulk endpoint now.")]
        public async Task<IActionResult> PostContents(string app, string schema, [FromBody] ImportContentsDto request)
        {
            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<BulkUpdateResult>();
            var response = result.Select(x => BulkResultDto.FromBulkResult(x, HttpContext)).ToArray();

            return Ok(response);
        }

        /// <summary>
        /// Bulk update content items.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="request">The bulk update request.</param>
        /// <returns>
        /// 201 => Contents created, update or delete.
        /// 400 => Contents request not valid.
        /// 404 => Contents references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/bulk")]
        [ProducesResponseType(typeof(BulkResultDto[]), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsReadOwn)]
        [ApiCosts(5)]
        public async Task<IActionResult> BulkUpdateContents(string app, string schema, [FromBody] BulkUpdateContentsDto request)
        {
            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<BulkUpdateResult>();
            var response = result.Select(x => BulkResultDto.FromBulkResult(x, HttpContext)).ToArray();

            return Ok(response);
        }

        /// <summary>
        /// Upsert a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to update.</param>
        /// <param name="request">The request parameters.</param>
        /// <returns>
        /// 200 => Content created or updated.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpsert)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostUpsertContent(string app, string schema, DomainId id, UpsertContentDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Update a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to update.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <returns>
        /// 200 => Content updated.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{schema}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdateOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string app, string schema, DomainId id, [FromBody] ContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Patchs a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to patch.</param>
        /// <param name="request">The patch for the content item.</param>
        /// <returns>
        /// 200 => Content patched.
        /// 400 => Content request not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPatch]
        [Route("content/{app}/{schema}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdateOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string app, string schema, DomainId id, [FromBody] ContentData request)
        {
            var command = new PatchContent { ContentId = id, Data = request };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Change status of a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to change.</param>
        /// <param name="request">The status request.</param>
        /// <returns>
        /// 200 => Content status changed.
        /// 400 => Content request not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{schema}/{id}/status/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsChangeStatusOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContentStatus(string app, string schema, DomainId id, [FromBody] ChangeStatusDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Cancel status change of a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to cancel.</param>
        /// <returns>
        /// 200 => Content status change cancelled.
        /// 400 => Content request not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{schema}/{id}/status/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsChangeStatusOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContentStatus(string app, string schema, DomainId id)
        {
            var command = new CancelContentSchedule { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Create a new draft version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to create the draft for.</param>
        /// <returns>
        /// 200 => Content draft created.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{schema}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsVersionCreateOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> CreateDraft(string app, string schema, DomainId id)
        {
            var command = new CreateContentDraft { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete the draft version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to delete the draft from.</param>
        /// <returns>
        /// 200 => Content draft deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{schema}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsDeleteOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteVersion(string app, string schema, DomainId id)
        {
            var command = new DeleteContentDraft { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="schema">The name of the schema.</param>
        /// <param name="id">The id of the content item to delete.</param>
        /// <param name="request">The request parameters.</param>
        /// <returns>
        /// 204 => Content deleted.
        /// 400 => Content cannot be deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can create an generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{schema}/{id}/")]
        [ApiPermissionOrAnonymous(Permissions.AppContentsDeleteOwn)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string app, string schema, DomainId id, DeleteContentDto request)
        {
            var command = request.ToCommand(id);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private async Task<ContentDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IEnrichedContentEntity>();
            var response = ContentDto.FromContent(result, Resources);

            return response;
        }

        private Q CreateQuery(string? ids, string? q)
        {
            return Q.Empty
                .WithIds(ids)
                .WithJsonQuery(q)
                .WithODataQuery(Request.QueryString.ToString());
        }
    }
}
