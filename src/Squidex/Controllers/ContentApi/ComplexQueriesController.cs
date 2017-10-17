using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using IQueryProvider = Squidex.Domain.Apps.Read.Contents.CustomQueries.IQueryProvider;

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public class ComplexQueriesController : ControllerBase
    {
        private readonly IContentQueryService contentQuery;
        private readonly IAssetRepository assetsRepository;
        private IQueryProvider queryProvider;

        public ComplexQueriesController(ICommandBus commandBus, IContentQueryService contentQuery, IAssetRepository assetsRepository, IQueryProvider queryProvider)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.assetsRepository = assetsRepository;
            this.queryProvider = queryProvider;
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/queries/{queryName}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, string queryName, [FromQuery] bool archived = false)
        {
            var schema = await contentQuery.FindSchemaAsync(App, name);

            if (schema == null)
            {
                return NotFound();
            }

            IQuery query = queryProvider.GetQueries(App, schema)?.FirstOrDefault();

            if (query == null)
            {
                return NotFound();
            }

            var context = new QueryContext(App, assetsRepository, contentQuery, User);
            var contents = await query.Execute(schema, context, HttpContext.Request.Query.ToDictionary(x => x.Key, x => (object)x.Value));

            var response = new AssetsDto
            {
                Total = contents.Count,
                Items = contents.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        itemModel.Data = item.Data.ToApiModel(schema.SchemaDef, App.LanguagesConfig, !User.IsFrontendClient());
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }
    }
}
