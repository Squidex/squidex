// ==========================================================================
//  ContentsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Core.Contents;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;

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

        public ContentsController(ICommandBus commandBus, ISchemaProvider schemas, IContentRepository contentRepository) 
            : base(commandBus)
        {
            this.schemas = schemas;

            this.contentRepository = contentRepository;
        }

        [HttpGet]
        [Route("content/{app}/{name}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, [FromQuery] bool nonPublished = false, [FromQuery] bool hidden = false)
        {
            var schemaEntity = await schemas.FindSchemaByNameAsync(AppId, name);

            if (schemaEntity == null)
            {
                return NotFound();
            }
            
            var query = Request.QueryString.ToString();

            var taskForContents = contentRepository.QueryAsync(schemaEntity.Id, nonPublished, query, App);
            var taskForCount    = contentRepository.CountAsync(schemaEntity.Id, nonPublished, query, App);

            await Task.WhenAll(taskForContents, taskForCount);

            var response = new AssetsDto
            {
                Total = taskForCount.Result,
                Items = taskForContents.Result.Take(200).Select(x =>
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
            var schemaEntity = await schemas.FindSchemaByNameAsync(AppId, name);

            if (schemaEntity == null)
            {
                return NotFound();
            }

            var entity = await contentRepository.FindContentAsync(schemaEntity.Id, id);

            if (entity == null)
            {
                return NotFound();
            }

            var resposne = SimpleMapper.Map(entity, new ContentDto());

            if (entity.Data != null)
            {
                resposne.Data = entity.Data.ToApiModel(schemaEntity.Schema, App.LanguagesConfig, null, hidden);
            }

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(resposne);
        }

        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent([FromBody] ContentData request, [FromQuery] bool publish = false)
        {
            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<ContentData>>();
            var response = ContentDto.Create(command, result);

            Response.Headers["ETag"] = new StringValues(response.Version.ToString());

            return CreatedAtAction(nameof(GetContent), new { id = response.Id }, response);
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(Guid id, [FromBody] ContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned() };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPatch]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(Guid id, [FromBody] ContentData request)
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
    }
}
