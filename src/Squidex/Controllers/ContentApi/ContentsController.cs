// ==========================================================================
//  ContentsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentsController : ControllerBase
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContentVersionLoader contentVersionLoader;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IContentVersionLoader contentVersionLoader,
            IGraphQLService graphQl)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.contentVersionLoader = contentVersionLoader;

            this.graphQl = graphQl;
        }

        [MustBeAppReader]
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql/")]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL([FromBody] GraphQLQuery query)
        {
            var result = await graphQl.QueryAsync(App, User, query);

            if (result.Errors?.Length > 0)
            {
                return BadRequest(new { result.Data, result.Errors });
            }
            else
            {
                return Ok(new { result.Data });
            }
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, [FromQuery] bool archived = false, [FromQuery] string ids = null)
        {
            HashSet<Guid> idsList = null;

            if (!string.IsNullOrWhiteSpace(ids))
            {
                idsList = new HashSet<Guid>();

                foreach (var id in ids.Split(','))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            var isFrontendClient = User.IsFrontendClient();

            var contents =
                idsList != null ?
                    await contentQuery.QueryWithCountAsync(App, name, User, archived, idsList) :
                    await contentQuery.QueryWithCountAsync(App, name, User, archived, Request.QueryString.ToString());

            var response = new AssetsDto
            {
                Total = contents.Total,
                Items = contents.Items.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        itemModel.Data = item.Data.ToApiModel(contents.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string name, Guid id)
        {
            var content = await contentQuery.FindContentAsync(App, name, User, id);

            var response = SimpleMapper.Map(content.Content, new ContentDto());

            if (content.Content.Data != null)
            {
                var isFrontendClient = User.IsFrontendClient();

                response.Data = content.Content.Data.ToApiModel(content.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
            }

            Response.Headers["ETag"] = new StringValues(content.Content.Version.ToString());

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string name, Guid id, int version)
        {
            var contentData = await contentVersionLoader.LoadAsync(App.Id, id, version);

            var response = contentData;

            Response.Headers["ETag"] = new StringValues(version.ToString());

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new CreateContent { ContentId = Guid.NewGuid(), User = User, Data = request.ToCleaned(), Publish = publish };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<NamedContentData>>();
            var response = ContentDto.Create(command, result);

            return CreatedAtAction(nameof(GetContent), new { id = command.ContentId }, response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new UpdateContent { ContentId = id, User = User, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPatch]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new PatchContent { ContentId = id, User = User, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new ChangeContentStatus { Status = Status.Published, ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new ChangeContentStatus { Status = Status.Draft, ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/archive/")]
        [ApiCosts(1)]
        public async Task<IActionResult> ArchiveContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new ChangeContentStatus { Status = Status.Archived, ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/restore/")]
        [ApiCosts(1)]
        public async Task<IActionResult> RestoreContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new ChangeContentStatus { Status = Status.Draft, ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpDelete]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new DeleteContent { ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}
