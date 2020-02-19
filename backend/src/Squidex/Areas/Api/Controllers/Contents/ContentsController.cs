// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
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
        /// <param name="query">The graphql query.</param>
        /// <returns>
        /// 200 => Contents retrieved or mutated.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [ApiPermission]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL(string app, [FromBody] GraphQLQuery query)
        {
            var (hasError, response) = await graphQl.QueryAsync(Context, query);

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
        /// 200 => Contents retrieved or mutated.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/batch")]
        [ApiPermission]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQLBatch(string app, [FromBody] GraphQLQuery[] batch)
        {
            var (hasError, response) = await graphQl.QueryAsync(Context, batch);

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
        /// 200 => Contents retrieved.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAllContents(string app, [FromQuery] string ids)
        {
            var contents = await contentQuery.QueryAsync(Context, Q.Empty.WithIds(ids).Ids);

            var response = Deferred.AsyncResponse(() =>
            {
                return ContentsDto.FromContentsAsync(contents, Context, this, null, contentWorkflow);
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
        /// 200 => Contents retrieved.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContents(string app, string name, [FromQuery] string? ids = null, [FromQuery] string? q = null)
        {
            var schema = await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var contents = await contentQuery.QueryAsync(Context, name,
                Q.Empty
                    .WithIds(ids)
                    .WithJsonQuery(q)
                    .WithODataQuery(Request.QueryString.ToString()));

            var response = Deferred.AsyncResponse(async () =>
            {
                return await ContentsDto.FromContentsAsync(contents, Context, this, schema, contentWorkflow);
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
        /// 200 => Content found.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string app, string name, Guid id)
        {
            var content = await contentQuery.FindContentAsync(Context, name, id);

            var response = ContentDto.FromContent(Context, content, this);

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
        /// 200 => Content found.
        /// 404 => Content, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiPermission(Permissions.AppContentsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string app, string name, Guid id, int version)
        {
            var content = await contentQuery.FindContentAsync(Context, name, id, version);

            var response = ContentDto.FromContent(Context, content, this);

            return Ok(response.Data);
        }

        /// <summary>
        /// Create a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <param name="publish">True to automatically publish the content.</param>
        /// <returns>
        /// 201 => Content created.
        /// 404 => Content, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ProducesResponseType(typeof(ContentsDto), 201)]
        [ApiPermission(Permissions.AppContentsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

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
        /// 404 => Content references, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/import")]
        [ProducesResponseType(typeof(ImportResultDto[]), 200)]
        [ApiPermission(Permissions.AppContentsCreate)]
        [ApiCosts(5)]
        public async Task<IActionResult> PostContent(string app, string name, [FromBody] ImportContentsDto request)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ImportResult>();
            var response = result.Select(x => ImportResultDto.FromImportResult(x, HttpContext)).ToArray();

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
        /// 404 => Content references, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string app, string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

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
        /// 404 => Content, schema or app not found.
        /// 400 => Content patch is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPatch]
        [Route("content/{app}/{name}/{id}/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string app, string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = new PatchContent { ContentId = id, Data = request.ToCleaned() };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Publish a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to publish.</param>
        /// <param name="request">The status request.</param>
        /// <returns>
        /// 200 => Content published.
        /// 404 => Content, schema or app not found.
        /// 400 => Request is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/status/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContentStatus(string app, string name, Guid id, ChangeStatusDto request)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Create a new version.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to discard changes.</param>
        /// <returns>
        /// 200 => Content restored.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission(Permissions.AppContentsVersionCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> CreateDraft(string app, string name, Guid id)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = new CreateContentDraft { ContentId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Discard changes.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to discard changes.</param>
        /// <returns>
        /// 200 => Content restored.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/draft/")]
        [ProducesResponseType(typeof(ContentsDto), 200)]
        [ApiPermission(Permissions.AppContentsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteVersion(string app, string name, Guid id)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

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
        /// <returns>
        /// 204 => Content deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can create an generated documentation for your app at /api/content/{appName}/docs.
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermission(Permissions.AppContentsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string app, string name, Guid id)
        {
            await contentQuery.GetSchemaOrThrowAsync(Context, name);

            var command = new DeleteContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private async Task<ContentDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IEnrichedContentEntity>();
            var response = ContentDto.FromContent(Context, result, this);

            return response;
        }
    }
}
