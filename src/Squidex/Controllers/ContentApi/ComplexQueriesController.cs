using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Extensibility;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    public class ComplexQueriesController : ControllerBase
    {
        private readonly IContentQueryService m_contentQuery;
        private readonly IAssetRepository m_assetsRepository;
        private IList<ISquidexPlugin> plugins;

        public ComplexQueriesController(IServiceProvider provider, ICommandBus commandBus, IContentQueryService contentQuery, IAssetRepository assetsRepository)
            : base(commandBus)
        {
            m_contentQuery = contentQuery;
            m_assetsRepository = assetsRepository;
            plugins = provider.GetServices<ISquidexPlugin>().ToList();
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/queries/{queryName}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, string queryName, [FromQuery] bool archived = false)
        {
            var schema = await m_contentQuery.FindSchemaAsync(App, name);

            if (schema == null)
            {
                return NotFound();
            }

            IQuery query = null;

            foreach (var squidexPlugin in plugins)
            {
                query = squidexPlugin.GetQueries(App, schema).FirstOrDefault(x => x.Name == queryName);
                if (query != null)
                {
                    break;
                }
            }

            if (query == null)
            {
                return NotFound();
            }

            var context = new QueryContext(App, m_assetsRepository, m_contentQuery, User);
            var contents = await query.Execute(context, null);

            var response = new AssetsDto
            {
                Total = contents.Total,
                Items = contents.Items.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        itemModel.Data = item.Data.ToApiModel(contents.Schema.SchemaDef, App.LanguagesConfig, !User.IsFrontendClient());
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }
    }
}
