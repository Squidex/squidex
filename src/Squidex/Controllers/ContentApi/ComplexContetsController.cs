using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.ComplexQueries;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Scripting;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    public class ComplexContetsController : ControllerBase
    {
        private readonly IContentQueryService contentQuery;
        private readonly IAssetRepository assetRepository;
        private readonly IQueryScriptFileService queryScriptFileService;

        public ComplexContetsController(
            IContentQueryService contentQuery,
            IAssetRepository assetRepository,
            IQueryScriptFileService queryScriptFileService,
            ICommandBus commandBus)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;
            this.assetRepository = assetRepository;
            this.queryScriptFileService = queryScriptFileService;
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/queries/{scriptName}")]
        [ApiCosts(4)]
        public async Task<IActionResult> GetContents(string name, string scriptName)
        {
            var complexQueryService = new ComplexQueryService(contentQuery, assetRepository);
            var scriptContents = queryScriptFileService.GetScriptContents(App.Name, name, scriptName);

            if (string.IsNullOrWhiteSpace(scriptContents))
            {
                return NotFound();
            }

            var result = await complexQueryService.QueryAsync(App, User, scriptContents, scriptName);

            return Ok(result);
        }
    }
}
