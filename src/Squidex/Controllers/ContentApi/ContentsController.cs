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
using Newtonsoft.Json.Linq;
using Squidex.Controllers.Api;
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
    [Authorize(Roles = "app-editor,app-owner,app-developer")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    public class ContentsController : ControllerBase
    {
        private readonly ISchemaProvider schemaProvider;
        private readonly IContentRepository contentRepository;

        public ContentsController(ICommandBus commandBus, ISchemaProvider schemaProvider, IContentRepository contentRepository) 
            : base(commandBus)
        {
            this.schemaProvider = schemaProvider;
            this.contentRepository = contentRepository;
        }

        [HttpGet]
        [Route("content/{app}/{name}")]
        public async Task<IActionResult> GetContents(string name, [FromQuery] string query = null, [FromQuery] int? take = null, [FromQuery] int? skip = null, [FromQuery] bool nonPublished = false, [FromQuery] bool hidden = false)
        {
            var schemaEntity = await schemaProvider.FindSchemaByNameAsync(AppId, name);

            if (schemaEntity == null)
            {
                return NotFound();
            }

            var taskForContents = contentRepository.QueryAsync(schemaEntity.Id, nonPublished, take, skip, query);
            var taskForCount = contentRepository.CountAsync(schemaEntity.Id, nonPublished, query);

            await Task.WhenAll(taskForContents, taskForCount);

            var model = new ContentsDto
            {
                Total = taskForCount.Result,
                Items = taskForContents.Result.Select(x =>
                {
                    var itemModel = SimpleMapper.Map(x, new ContentDto());

                    if (x.Data != null)
                    {
                        itemModel.Data = x.Data.ToApiResponse(schemaEntity.Schema, App.Languages, App.MasterLanguage);
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
            var schemaEntity = await schemaProvider.FindSchemaByNameAsync(AppId, name);

            if (schemaEntity == null)
            {
                return NotFound();
            }

            var content = await contentRepository.FindContentAsync(schemaEntity.Id, id);

            if (content == null)
            {
                return NotFound();
            }

            var model = SimpleMapper.Map(content, new ContentDto());

            if (content.Data != null)
            {
                model.Data = content.Data.ToApiResponse(schemaEntity.Schema, App.Languages, App.MasterLanguage, hidden);
            }

            return Ok(model);
        }

        [HttpPost]
        [Route("content/{app}/{name}/")]
        public async Task<IActionResult> PostContent([FromBody] Dictionary<string, Dictionary<string, JToken>> request)
        {
            var command = new CreateContent { Data = ContentData.FromApiRequest(request), AggregateId = Guid.NewGuid() };

            var context = await CommandBus.PublishAsync(command);
            var result = context.Result<Guid>();

            return CreatedAtAction(nameof(GetContent), new { id = result }, new EntityCreatedDto { Id = result.ToString() });
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        public async Task<IActionResult> PutContent(Guid id, [FromBody] Dictionary<string, Dictionary<string, JToken>> request)
        {
            var command = new UpdateContent { AggregateId = id, Data = ContentData.FromApiRequest(request) };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish")]
        public async Task<IActionResult> PublishContent(Guid id)
        {
            var command = new PublishContent { AggregateId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish")]
        public async Task<IActionResult> UnpublishContent(Guid id)
        {
            var command = new UnpublishContent { AggregateId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [HttpDelete]
        [Route("content/{app}/{name}/{id}")]
        public async Task<IActionResult> PutContent(Guid id)
        {
            var command = new DeleteContent { AggregateId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}
