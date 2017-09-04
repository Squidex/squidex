// ==========================================================================
//  ContentSwaggerController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Generator;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [SwaggerIgnore]
    public sealed class ContentSwaggerController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        private readonly SchemasSwaggerGenerator schemasSwaggerGenerator;

        public ContentSwaggerController(ICommandBus commandBus, ISchemaRepository schemaRepository, SchemasSwaggerGenerator schemasSwaggerGenerator)
            : base(commandBus)
        {
            this.schemaRepository = schemaRepository;
            this.schemasSwaggerGenerator = schemasSwaggerGenerator;
        }

        [HttpGet]
        [Route("content/{app}/docs/")]
        [ApiCosts(0)]
        public IActionResult Docs(string app)
        {
            ViewBag.Specification = $"~/content/{app}/swagger/v1/swagger.json";

            return View("Docs");
        }

        [HttpGet]
        [Route("content/{app}/swagger/v1/swagger.json")]
        [ApiCosts(0)]
        public async Task<IActionResult> GetSwagger(string app)
        {
            var schemas = await schemaRepository.QueryAllAsync(App.Id);

            var swaggerDocument = await schemasSwaggerGenerator.Generate(App, schemas);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
