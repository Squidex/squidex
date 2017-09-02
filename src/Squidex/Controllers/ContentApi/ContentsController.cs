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
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

// ReSharper disable InvertIf
// ReSharper disable PossibleNullReferenceException
// ReSharper disable RedundantIfElseBlock

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentsController : ControllerBase
    {
        private readonly ISchemaProvider schemas;
        private readonly IScriptEngine scriptEngine;
        private readonly IContentRepository contentRepository;
        private readonly IGraphQLService graphQL;

        public ContentsController(
            ICommandBus commandBus,
            ISchemaProvider schemas, 
            IScriptEngine scriptEngine,
            IContentRepository contentRepository,
            IGraphQLService graphQL)
            : base(commandBus)
        {
            this.graphQL = graphQL;
            this.scriptEngine = scriptEngine;
            this.schemas = schemas;
            this.contentRepository = contentRepository;
        }

        [MustBeAppReader]
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql")]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL([FromBody] GraphQLQuery query)
        {
            var result = await graphQL.QueryAsync(App, query);

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
        [Route("content/{app}/{name}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, [FromQuery] string ids = null)
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

            var isFrontendClient = User.IsFrontendClient();

            var query = Request.QueryString.ToString();

            var taskForItems = contentRepository.QueryAsync(App, schemaEntity.Id, isFrontendClient, idsList, query);
            var taskForCount = contentRepository.CountAsync(App, schemaEntity.Id, isFrontendClient, idsList, query);

            await Task.WhenAll(taskForItems, taskForCount);

            var scriptUser = ScriptUser.Create(User);
            var scriptText = schemaEntity.ScriptQuery;

            var hasScript = !string.IsNullOrWhiteSpace(scriptText);

            var response = new AssetsDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        var data = item.Data.ToApiModel(schemaEntity.Schema, App.LanguagesConfig, null, !isFrontendClient);

                        if (hasScript && !isFrontendClient)
                        {
                            data = scriptEngine.ExecuteAndTransform(new ScriptContext { Data = data, ContentId = item.Id, User = scriptUser }, scriptText, "transform item");
                        }

                        itemModel.Data = data;
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string name, Guid id)
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
                var isFrontendClient = User.IsFrontendClient();

                var data = entity.Data.ToApiModel(schemaEntity.Schema, App.LanguagesConfig, null, !isFrontendClient);

                if (!isFrontendClient)
                {
                    var scriptUser = ScriptUser.Create(User);
                    var scriptText = schemaEntity.ScriptQuery;

                    var hasScript = !string.IsNullOrWhiteSpace(scriptText);

                    if (hasScript)
                    {
                        data = scriptEngine.ExecuteAndTransform(new ScriptContext { Data = data, ContentId = entity.Id, User = scriptUser }, scriptText, "transform item");
                    }
                }

                response.Data = data;
            }

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent([FromBody] NamedContentData request, [FromQuery] bool publish = false)
        {
            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Publish = publish };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<NamedContentData>>();
            var response = result.IdOrValue;

            return CreatedAtAction(nameof(GetContent), new { id = command.ContentId }, response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(Guid id, [FromBody] NamedContentData request)
        {
            var command = new UpdateContent { ContentId = id, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPatch]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(Guid id, [FromBody] NamedContentData request)
        {
            var command = new PatchContent { ContentId = id, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(Guid id)
        {
            var command = new PublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(Guid id)
        {
            var command = new UnpublishContent { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
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
