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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Squidex.Controllers.Api;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Core.Contents;
using Squidex.Core.Identity;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.Schemas.Services;
using Squidex.Write.Contents.Commands;

namespace Squidex.Controllers.ContentApi
{
    [Authorize(Roles = SquidexRoles.AppEditor)]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
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
        public async Task<IActionResult> GetContents(string name, [FromQuery] bool nonPublished = false, [FromQuery] bool hidden = false)
        {
            var schemaEntity = await schemas.FindSchemaByNameAsync(AppId, name);

            if (schemaEntity == null)
            {
                return NotFound();
            }

            var languages = new HashSet<Language>(App.Languages);

            var query = Request.QueryString.ToString();

            var taskForContents = contentRepository.QueryAsync(schemaEntity.Id, nonPublished, query, languages);
            var taskForCount = contentRepository.CountAsync(schemaEntity.Id, nonPublished, query, languages);

            await Task.WhenAll(taskForContents, taskForCount);

            var model = new ContentsDto
            {
                Total = taskForCount.Result,
                Items = taskForContents.Result.Take(200).Select(x =>
                {
                    var itemModel = SimpleMapper.Map(x, new ContentDto());

                    if (x.Data != null)
                    {
                        itemModel.Data = x.Data.ToApiModel(schemaEntity.Schema, App.Languages, App.MasterLanguage);
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(model);
        }

        [HttpGet]
        [Route("content/{app}/{name}/{id}")]
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

            var model = SimpleMapper.Map(entity, new ContentDto());

            if (entity.Data != null)
            {
                model.Data = entity.Data.ToApiModel(schemaEntity.Schema, App.Languages, App.MasterLanguage, hidden);
            }

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(model);
        }

        [HttpPost]
        [Route("content/{app}/{name}/")]
        public async Task<IActionResult> PostContent([FromBody] ContentData request)
        {
            var command = new CreateContent { Data = request, ContentId = Guid.NewGuid() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>().IdOrValue;
            var response = new EntityCreatedDto { Id = result.ToString() };

            return CreatedAtAction(nameof(GetContent), new { id = result }, response);
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        public async Task<IActionResult> PutContent(Guid id, [FromBody] ContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPatch]
        [Route("content/{app}/{name}/{id}")]
        public async Task<IActionResult> PatchContent(Guid id, [FromBody] ContentData request)
        {
            var command = new PatchContent { ContentId = id, Data = request };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish")]
        public async Task<IActionResult> PublishContent(Guid id)
        {
            var command = new PublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish")]
        public async Task<IActionResult> UnpublishContent(Guid id)
        {
            var command = new UnpublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpDelete]
        [Route("content/{app}/{name}/{id}")]
        public async Task<IActionResult> PutContent(Guid id)
        {
            var command = new DeleteContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}
