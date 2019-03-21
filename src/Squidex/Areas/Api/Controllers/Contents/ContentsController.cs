﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NodaTime;
using NodaTime.Text;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Areas.Api.Controllers.Contents
{
    public sealed class ContentsController : ApiController
    {
        private readonly IOptions<MyContentsControllerOptions> controllerOptions;
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IGraphQLService graphQl,
            IOptions<MyContentsControllerOptions> controllerOptions)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.controllerOptions = controllerOptions;

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
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [ApiPermission]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL(string app, [FromBody] GraphQLQuery query)
        {
            var result = await graphQl.QueryAsync(Context(), query);

            if (result.HasError)
            {
                return BadRequest(result.Response);
            }
            else
            {
                return Ok(result.Response);
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
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/batch")]
        [ApiPermission]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQLBatch(string app, [FromBody] GraphQLQuery[] batch)
        {
            var result = await graphQl.QueryAsync(Context(), batch);

            if (result.HasError)
            {
                return BadRequest(result.Response);
            }
            else
            {
                return Ok(result.Response);
            }
        }

        /// <summary>
        /// Queries contents.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="ids">The optional ids of the content to fetch.</param>
        /// <param name="archived">Indicates whether to query content items from the archive.</param>
        /// <returns>
        /// 200 => Contents retrieved.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/")]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContents(string app, string name, [FromQuery] bool archived = false, [FromQuery] string ids = null)
        {
            var context = Context().WithArchived(archived);

            var result = await contentQuery.QueryAsync(context, name, Q.Empty.WithIds(ids).WithODataQuery(Request.QueryString.ToString()));

            var response = new ContentsDto
            {
                Total = result.Total,
                Items = result.Take(200).Select(x => ContentDto.FromContent(x, context)).ToArray()
            };

            if (controllerOptions.Value.EnableSurrogateKeys && response.Items.Length <= controllerOptions.Value.MaxItemsForSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = response.Items.ToSurrogateKeys();
            }

            Response.Headers[HeaderNames.ETag] = response.Items.ToManyEtag(response.Total);

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
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermission]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string app, string name, Guid id)
        {
            var context = Context();
            var content = await contentQuery.FindContentAsync(context, name, id);

            var response = ContentDto.FromContent(content, context);

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = content.Id.ToString();
            }

            Response.Headers[HeaderNames.ETag] = content.Version.ToString();

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
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpGet]
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiPermission(Permissions.AppContentsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string app, string name, Guid id, int version)
        {
            var context = Context();
            var content = await contentQuery.FindContentAsync(context, name, id, version);

            var response = ContentDto.FromContent(content, context);

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = content.Id.ToString();
            }

            Response.Headers[HeaderNames.ETag] = content.Version.ToString();

            return Ok(response.Data);
        }

        /// <summary>
        /// Create a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <param name="publish">Indicates whether the content should be published immediately.</param>
        /// <returns>
        /// 201 => Content created.
        /// 404 => Content, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiPermission(Permissions.AppContentsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var publishPermission = Permissions.ForApp(Permissions.AppContentsPublish, app, name);

            if (publish && !User.Permissions().Includes(publishPermission))
            {
                return new StatusCodeResult(123);
            }

            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<NamedContentData>>();
            var response = ContentDto.FromCommand(command, result);

            return CreatedAtAction(nameof(GetContent), new { id = command.ContentId }, response);
        }

        /// <summary>
        /// Update a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to update.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <param name="asDraft">Indicates whether the update is a proposal.</param>
        /// <returns>
        /// 200 => Content updated.
        /// 404 => Content, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermission(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string app, string name, Guid id, [FromBody] NamedContentData request, [FromQuery] bool asDraft = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned(), AsDraft = asDraft };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        /// <summary>
        /// updates orderno values of newly sorted items
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to update.</param>
        /// <param name="request">The full data for the content item.</param>
        /// <param name="asDraft">Indicates whether the update is a proposal.</param>
        /// <returns>
        /// 200 => Content updated.
        /// 404 => Content, schema or app not found.
        /// 400 => Content data is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPost]
        [Route("content/{app}/{name}/updateOrderNo/{id}/{order}")]
        [ApiPermission(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> UpdateOrderNo(string app, string name, Guid id, long order)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = new UpdateContentOrderNo { ContentId = id, NewOrderNo = order };
            var context = await CommandBus.PublishAsync(command);
            return Ok();
        }

        /// <summary>
        /// Patchs a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to patch.</param>
        /// <param name="request">The patch for the content item.</param>
        /// <param name="asDraft">Indicates whether the patch is a proposal.</param>
        /// <returns>
        /// 200 => Content patched.
        /// 404 => Content, schema or app not found.
        /// 400 => Content patch is not valid.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPatch]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermission(Permissions.AppContentsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string app, string name, Guid id, [FromBody] NamedContentData request, [FromQuery] bool asDraft = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = new PatchContent { ContentId = id, Data = request.ToCleaned(), AsDraft = asDraft };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        /// <summary>
        /// Publish a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to publish.</param>
        /// <param name="dueTime">The date and time when the content should be published.</param>
        /// <returns>
        /// 204 => Content published.
        /// 404 => Content, schema or app not found.
        /// 400 => Content was already published.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish/")]
        [ApiPermission(Permissions.AppContentsPublish)]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = CreateCommand(id, Status.Published, dueTime);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Unpublish a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to unpublish.</param>
        /// <param name="dueTime">The date and time when the content should be unpublished.</param>
        /// <returns>
        /// 204 => Content unpublished.
        /// 404 => Content, schema or app not found.
        /// 400 => Content was not published.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish/")]
        [ApiPermission(Permissions.AppContentsUnpublish)]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = CreateCommand(id, Status.Draft, dueTime);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Archive a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to archive.</param>
        /// <param name="dueTime">The date and time when the content should be archived.</param>
        /// <returns>
        /// 204 => Content archived.
        /// 404 => Content, schema or app not found.
        /// 400 => Content was already archived.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/archive/")]
        [ApiPermission(Permissions.AppContentsArchive)]
        [ApiCosts(1)]
        public async Task<IActionResult> ArchiveContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = CreateCommand(id, Status.Archived, dueTime);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Restore a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to restore.</param>
        /// <param name="dueTime">The date and time when the content should be restored.</param>
        /// <returns>
        /// 204 => Content restored.
        /// 404 => Content, schema or app not found.
        /// 400 => Content was not archived.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/restore/")]
        [ApiPermission(Permissions.AppContentsRestore)]
        [ApiCosts(1)]
        public async Task<IActionResult> RestoreContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = CreateCommand(id, Status.Draft, dueTime);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Discard changes.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to discard changes.</param>
        /// <returns>
        /// 204 => Content restored.
        /// 404 => Content, schema or app not found.
        /// 400 => Content was not archived.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpPut]
        [Route("content/{app}/{name}/{id}/discard/")]
        [ApiPermission(Permissions.AppContentsDiscard)]
        [ApiCosts(1)]
        public async Task<IActionResult> DiscardChanges(string app, string name, Guid id)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = new DiscardChanges { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete a content item.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="id">The id of the content item to delete.</param>
        /// <returns>
        /// 204 => Content has been deleted.
        /// 404 => Content, schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can create an generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/")]
        [ApiPermission(Permissions.AppContentsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string app, string name, Guid id)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context(), name);

            var command = new DeleteContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private static ChangeContentStatus CreateCommand(Guid id, Status status, string dueTime)
        {
            Instant? dt = null;

            if (!string.IsNullOrWhiteSpace(dueTime))
            {
                var parseResult = InstantPattern.General.Parse(dueTime);

                if (parseResult.Success)
                {
                    dt = parseResult.Value;
                }
            }

            return new ChangeContentStatus { Status = status, ContentId = id, DueTime = dt };
        }

        private QueryContext Context()
        {
            return QueryContext.Create(App, User)
                .WithAssetUrlsToResolve(Request.Headers["X-Resolve-Urls"])
                .WithFlatten(Request.Headers.ContainsKey("X-Flatten"))
                .WithLanguages(Request.Headers["X-Languages"])
                .WithUnpublished(Request.Headers.ContainsKey("X-Unpublished"));
        }
    }
}
