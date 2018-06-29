// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents
{
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentsController : ApiController
    {
        private readonly IOptions<ContentsControllerOptions> controllerOptions;
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IGraphQLService graphQl,
            IOptions<ContentsControllerOptions> controllerOptions)
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
        /// <param name="query">The graphql endpoint.</param>
        /// <returns>
        /// 200 => Contents retrieved or mutated.
        /// 404 => Schema or app not found.
        /// </returns>
        /// <remarks>
        /// You can read the generated documentation for your app at /api/content/{appName}/docs
        /// </remarks>
        [MustBeAppReader]
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL(string app, [FromBody] GraphQLQuery query)
        {
            var result = await graphQl.QueryAsync(Context(), query);

            if (result.Errors?.Length > 0)
            {
                return BadRequest(new { result.Data, result.Errors });
            }
            else
            {
                return Ok(new { result.Data });
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
        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string app, string name, [FromQuery] bool archived = false, [FromQuery] string ids = null)
        {
            List<Guid> idsList = null;

            if (!string.IsNullOrWhiteSpace(ids))
            {
                idsList = new List<Guid>();

                foreach (var id in ids.Split(','))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            var context = Context().WithSchemaName(name).WithArchived(archived);

            var result =
                idsList?.Count > 0 ?
                    await contentQuery.QueryAsync(context, idsList) :
                    await contentQuery.QueryAsync(context, Request.QueryString.ToString());

            var response = new ContentsDto
            {
                Total = result.Total,
                Items = result.Take(200).Select(x => ContentDto.FromContent(x, context)).ToArray()
            };

            var options = controllerOptions.Value;

            if (options.EnableSurrogateKeys && response.Items.Length <= options.MaxItemsForSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = string.Join(" ", response.Items.Select(x => x.Id));
            }

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
        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string app, string name, Guid id)
        {
            var context = Context().WithSchemaName(name);
            var content = await contentQuery.FindContentAsync(context, id);

            var response = ContentDto.FromContent(content, context);

            Response.Headers["ETag"] = content.Version.ToString();

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = content.Id.ToString();
            }

            return Ok(response);
        }

        /// <summary>
        /// Get a content item with a specific version.
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
        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string app, string name, Guid id, int version)
        {
            var context = Context().WithSchemaName(name);
            var content = await contentQuery.FindContentAsync(context, id, version);

            var response = ContentDto.FromContent(content, context);

            Response.Headers["ETag"] = content.Version.ToString();

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = content.Id.ToString();
            }

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
        [MustBeAppEditor]
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string app, string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string app, string name, Guid id, [FromBody] NamedContentData request, [FromQuery] bool asDraft = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned(), AsDraft = asDraft };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
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
        [MustBeAppEditor]
        [HttpPatch]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string app, string name, Guid id, [FromBody] NamedContentData request, [FromQuery] bool asDraft = false)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/archive/")]
        [ApiCosts(1)]
        public async Task<IActionResult> ArchiveContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/restore/")]
        [ApiCosts(1)]
        public async Task<IActionResult> RestoreContent(string app, string name, Guid id, string dueTime = null)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

            var command = CreateCommand(id, Status.Draft, dueTime);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Discard changes of a content item.
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
        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/discard/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DiscardChanges(string app, string name, Guid id)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
        [MustBeAppEditor]
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string app, string name, Guid id)
        {
            await contentQuery.ThrowIfSchemaNotExistsAsync(Context().WithSchemaName(name));

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
            return QueryContext.Create(App, User, Request.Headers["X-Languages"]).WithFlatten(Request.Headers.ContainsKey("X-Flatten"));
        }
    }
}
