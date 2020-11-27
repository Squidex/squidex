// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents
{
    public sealed class ContentsController : ApiController
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContentWorkflow contentWorkflow;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IContentWorkflow contentWorkflow,
            IGraphQLService graphQl)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.contentWorkflow = contentWorkflow;

            this.graphQl = graphQl;
        }

        /// <summary>
        /// GraphQL endpoint.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="queries">The graphql query.</param>
        /// <returns>
        /// 200 => Contents returned or mutated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/graphql/")]
        [ApiPermissionOrAnonymous]
        [ApiCosts(2)]
        public async Task<IActionResult> GetGraphQL(string app, [FromQuery] GraphQLGetDto? queries = null)
        {
            var request = queries?.ToQuery() ?? new GraphQLQuery();

            var (hasError, response) = await graphQl.QueryAsync(Context, request);

            if (hasError)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        /// <summary>
        /// GraphQL endpoint.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="query">The graphql query.</param>
        /// <returns>
        /// 200 => Contents returned or mutated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [ApiPermissionOrAnonymous]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL(string app, [FromBody] GraphQLPostDto query)
        {
            var request = query?.ToQuery() ?? new GraphQLQuery();

            var (hasError, response) = await graphQl.QueryAsync(Context, request);

            if (hasError)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        /// <summary>
        /// GraphQL endpoint (Batch).
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="batch">The graphql queries.</param>
        /// <returns>
        /// 200 => Contents returned or mutated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/graphql/batch")]
        [ApiPermissionOrAnonymous]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQLBatch(string app, [FromBody] GraphQLPostDto[] batch)
        {
            var request = batch.Select(x => x.ToQuery()).ToArray();

            var (hasError, response) = await graphQl.QueryAsync(Context, request);

            if (hasError)
            {
                return BadRequest(response);
            }
            else
            {
                return Ok(response);
            }
        }

        /// <summary>
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="ids">The optional ids of the content to fetch.</param>
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAllContents(string app, [FromQuery] string ids)
        {
            var contents = await contentQuery.QueryAsync(Context, Q.Empty.WithIds(ids).Ids);

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
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAllContentsPost(string app, [FromBody] ContentsIdsQueryDto query)
        {
            var contents = await contentQuery.QueryAsync(Context, query.Ids);

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
        /// <param name="name">The name of the schema.</param>
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
        [Route("content/{app}/{name}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContents(string app, string name, [FromQuery] string? ids = null, [FromQuery] string? q = null)
        {
            var schema = await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var contents = await contentQuery.QueryAsync(Context, name, CreateQuery(ids, q));

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, schema, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="query">The required query object.</param>
        /// <returns>
        /// 200 => Contents returned.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/query")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentsPost(string app, string name, [FromBody] QueryDto query)
        {
            var schema = await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var contents = await contentQuery.QueryAsync(Context, name, query?.ToQuery() ?? Q.Empty);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Resources, schema, contentWorkflow);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content to fetch.</param>
        /// <returns>
        /// 200 => Content returned.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string app, string name, DomainId id)
        {
            var content = await contentQuery.FindAsync(Context, name, id);

            var response = ContentDto.FromContent(content, Resources);

            return Ok(response);
        }

        /// <summary>
        /// Get a content by version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
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
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiPermissionOrAnonymous(Permissions.AppContentsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string app, string name, DomainId id, int version)
        {
            var content = await contentQuery.FindAsync(Context, name, id, version);

            var response = ContentDto.FromContent(content, Resources);

            return Ok(response.Data);
        }

        /// <summary>
        /// Create a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <param name="publish">True to automatically publish the content.</param>
        /// <param name="id">The optional custom content id.</param>
        /// <returns>
        /// 201 => Content created.
        /// 400 => Content request not valid.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ProducesResponseType(typeof(ContentsDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false, [FromQuery] DomainId? id = null)
        {
            var command = new CreateContent { Data = request.ToCleaned(), Publish = publish };

            if (id != null && id.Value != default && !string.IsNullOrWhiteSpace(id.Value.ToString()))
            {
                command.ContentId = id.Value;
            }

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetContent), new { app, name, id = command.ContentId }, response);
        }

        /// <summary>
        /// Import content items.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The import request.</param>
        /// <returns>
        /// 201 => Contents created.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/import")]
        [ProducesResponseType(typeof(BulkResultDto[]), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsCreate)]
        [ApiCosts(5)]
        public async Task<IActionResult> PostContents(string app, string name, [FromBody] ImportContentsDto request)
        {
            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<BulkUpdateResult>();
            var response = result.Select(x => BulkResultDto.FromImportResult(x, HttpContext)).ToArray();

            return Ok(response);
        }

        /// <summary>
        /// Bulk update content items.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The bulk update request.</param>
        /// <returns>
        /// 201 => Contents created.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/bulk")]
        [ProducesResponseType(typeof(BulkResultDto[]), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContents)]
        [ApiCosts(5)]
        public async Task<IActionResult> BulkContents(string app, string name, [FromBody] BulkUpdateDto request)
        {
            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<BulkUpdateResult>();
            var response = result.Select(x => BulkResultDto.FromImportResult(x, HttpContext)).ToArray();

            return Ok(response);
        }

        /// <summary>
        /// Upsert a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to update.</param>
        /// <param name="publish">True to automatically publish the content.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <returns>
        /// 200 => Content updated.
        /// 400 => Content request not valid.
        /// 404 => Content references, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string name, DomainId id, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            var command = new UpsertContent { ContentId = id, Data = request.ToCleaned(), Publish = publish };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Update a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
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
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string app, string name, DomainId id, [FromBody] NamedContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned() };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Patchs a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
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
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string app, string name, DomainId id, [FromBody] NamedContentData request)
        {
            var command = new PatchContent { ContentId = id, Data = request.ToCleaned() };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Change status of a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
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
        [Route("content/{app}/{name}/{id}/status/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContentStatus(string app, string name, DomainId id, ChangeStatusDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Create a new draft version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to create the draft for.</param>
        /// <returns>
        /// 200 => Content draft created.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsVersionCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> CreateDraft(string app, string name, DomainId id)
        {
            var command = new CreateContentDraft { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete the draft version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to delete the draft from.</param>
        /// <returns>
        /// 200 => Content draft deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermissionOrAnonymous(Permissions.AppContentsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteVersion(string app, string name, DomainId id)
        {
            var command = new DeleteContentDraft { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to delete.</param>
        /// <param name="checkReferrers">True to check referrers of this content.</param>
        /// <returns>
        /// 204 => Content deleted.
        /// 400 => Content cannot be deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can create an generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermissionOrAnonymous(Permissions.AppContentsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string app, string name, DomainId id, [FromQuery] bool checkReferrers = false)
        {
            var command = new DeleteContent { ContentId = id, CheckReferrers = checkReferrers };

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
