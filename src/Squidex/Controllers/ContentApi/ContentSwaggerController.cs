// ==========================================================================
//  ContentSwaggerController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Generator;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentSwaggerController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        private readonly ICustomQueryProvider queryProvider;
        private readonly SchemasSwaggerGenerator schemasSwaggerGenerator;

        public ContentSwaggerController(
            ICommandBus commandBus,
            ICustomQueryProvider queryProvider,
            ISchemaRepository schemaRepository,
            SchemasSwaggerGenerator schemasSwaggerGenerator)
            : base(commandBus)
        {
            this.queryProvider = queryProvider;
            this.schemaRepository = schemaRepository;
            this.schemasSwaggerGenerator = schemasSwaggerGenerator;
        }

        [HttpGet]
        [Route("content/{app}/docs/")]
        [ApiCosts(0)]
        public IActionResult Docs(string app)
        {
            var vm = new DocsVM { Specification = $"~/content/{app}/swagger/v1/swagger.json" };

            return View(nameof(Docs), vm);
        }

        [HttpGet]
        [Route("content/{app}/swagger/v1/swagger.json")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSwagger(string app)
        {
            var taskForSchemas = schemaRepository.QueryAllAsync(App.Id);
            var taskForQueries = queryProvider.GetQueriesAsync(App);

            await Task.WhenAll(taskForSchemas, taskForQueries);

            var swaggerDocument =
                await schemasSwaggerGenerator.GenerateAsync(App,
                    taskForSchemas.Result.Where(x => x.IsPublished),
                    taskForQueries.Result);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
