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
using NodaTime;
using NodaTime.Text;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents
{
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentsController : ApiController
    {
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus,
            IContentQueryService contentQuery,
            IGraphQLService graphQl)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;

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

            var result =
                idsList?.Count > 0 ?
                    await contentQuery.QueryAsync(App, name, User, archived, idsList) :
                    await contentQuery.QueryAsync(App, name, User, archived, Request.QueryString.ToString());

            var response = new ContentsDto
            {
                Total = result.Contents.Total,
                Items = result.Contents.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        itemModel.Data = item.Data.ToApiModel(result.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
                    }

                    return itemModel;
                }).ToArray()
            };

            Response.Headers["Surrogate-Key"] = string.Join(" ", response.Items.Select(x => x.Id));

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string name, Guid id)
        {
            var (schema, entity) = await contentQuery.FindContentAsync(App, name, User, id);

            var response = SimpleMapper.Map(entity, new ContentDto());

            if (entity.Data != null)
            {
                var isFrontendClient = User.IsFrontendClient();

                response.Data = entity.Data.ToApiModel(schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
            }

            Response.Headers["ETag"] = entity.Version.ToString();
            Response.Headers["Surrogate-Key"] = entity.Id.ToString();

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}/{version}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContentVersion(string name, Guid id, int version)
        {
            var content = await contentQuery.FindContentAsync(App, name, User, id, version);

            var response = SimpleMapper.Map(content.Content, new ContentDto());

            if (content.Content.Data != null)
            {
                var isFrontendClient = User.IsFrontendClient();

                response.Data = content.Content.Data.ToApiModel(content.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
            }

            Response.Headers["ETag"] = version.ToString();

            return Ok(response.Data);
        }

        [MustBeAppEditor]
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent(string name, [FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

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

            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned() };

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

            var command = new PatchContent { ContentId = id, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(string name, Guid id, string dueDate = null)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = CreateCommand(id, Status.Published, dueDate);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish/")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(string name, Guid id, string dueDate = null)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = CreateCommand(id, Status.Draft, dueDate);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/archive/")]
        [ApiCosts(1)]
        public async Task<IActionResult> ArchiveContent(string name, Guid id, string dueDate = null)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = CreateCommand(id, Status.Archived, dueDate);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/restore/")]
        [ApiCosts(1)]
        public async Task<IActionResult> RestoreContent(string name, Guid id, string dueDate = null)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = CreateCommand(id, Status.Draft, dueDate);

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

            var command = new DeleteContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private static ChangeContentStatus CreateCommand(Guid id, Status status, string dueDate)
        {
            Instant? dt = null;

            if (string.IsNullOrWhiteSpace(dueDate))
            {
                var parseResult = InstantPattern.General.Parse(dueDate);

                if (!parseResult.Success)
                {
                    dt = parseResult.Value;
                }
            }

            return new ChangeContentStatus { Status = status, ContentId = id, DueDate = dt };
        }
    }
}
