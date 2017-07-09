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
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    [MustBeAppEditor]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public class ContentsController : ControllerBase
    {
        private readonly ISchemaProvider schemas;
        private readonly IContentRepository contentRepository;
        private readonly IGraphQLInvoker graphQL;

        public ContentsController(
            ICommandBus commandBus, 
            ISchemaProvider schemas,
            IContentRepository contentRepository,
            IGraphQLInvoker graphQL) 
            : base(commandBus)
        {
            this.graphQL = graphQL;
            this.schemas = schemas;
            this.contentRepository = contentRepository;
        }

        [HttpGet]
        [Route("content/{app}/graphql")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetGraphQL([FromQuery] GraphQLQuery query)
        {
            var result = await graphQL.QueryAsync(App, query);

            return Ok(result);
        }

        [HttpPost]
        [Route("content/{app}/graphql")]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL([FromBody] GraphQLQuery query)
        {
            var result = await graphQL.QueryAsync(App, query);

            return Ok(result);
        }

        [HttpGet]
        [Route("content/{app}/{name}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, [FromQuery] bool nonPublished = false, [FromQuery] bool hidden = false, [FromQuery] string ids = null)
        {
            var schemaEntity = await FindSchemaAsync(name);

            var idsList = new HashSet<Guid>();

            if (!string.IsNullOrWhiteSpace(ids))
            {
                foreach (var id in ids.Split(','))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            var query = Request.QueryString.ToString();

            var taskForItems = contentRepository.QueryAsync(App, schemaEntity.Id, nonPublished, idsList, query);
            var taskForCount = contentRepository.CountAsync(App, schemaEntity.Id, nonPublished, idsList, query);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new AssetsDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Take(200).Select(x =>
                {
                    var itemModel = SimpleMapper.Map(x, new ContentDto());

                    if (x.Data != null)
                    {
                        itemModel.Data = x.Data.ToApiModel(schemaEntity.Schema, App.LanguagesConfig);
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string name, Guid id, bool hidden = false)
        {
            var schemaEntity = await FindSchemaAsync(name);

            if (schemaEntity == null)
            {
                return NotFound();
            }

            var entity = await contentRepository.FindContentAsync(App, schemaEntity.Id, id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = SimpleMapper.Map(entity, new ContentDto());

            if (entity.Data != null)
            {
                response.Data = entity.Data.ToApiModel(schemaEntity.Schema, App.LanguagesConfig, null, hidden);
            }

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(response);
        }

        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent([FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<NamedContentData>>();
            var response = ContentDto.Create(command, result);

            return CreatedAtAction(nameof(GetContent), new { id = response.Id }, response);
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(Guid id, [FromBody] NamedContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned() };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPatch]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(Guid id, [FromBody] NamedContentData request)
        {
            var command = new PatchContent { ContentId = id, Data = request.ToCleaned() };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(Guid id)
        {
            var command = new PublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(Guid id)
        {
            var command = new UnpublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpDelete]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(Guid id)
        {
            var command = new DeleteContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        private async Task<ISchemaEntity> FindSchemaAsync(string name)
        {
            ISchemaEntity schemaEntity;

            if (Guid.TryParse(name, out var schemaId))
            {
                schemaEntity = await schemas.FindSchemaByIdAsync(schemaId);
            }
            else
            {
                schemaEntity = await schemas.FindSchemaByNameAsync(AppId, name);
            }

            return schemaEntity;
        }
    }
}
