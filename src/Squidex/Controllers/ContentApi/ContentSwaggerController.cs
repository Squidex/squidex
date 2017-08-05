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
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Pipeline;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [SwaggerIgnore]
    public class ContentSwaggerController : Controller
    {
        private readonly ISchemaRepository schemaRepository;
        private readonly IAppProvider appProvider;
        private readonly SchemasSwaggerGenerator schemasSwaggerGenerator;

        public ContentSwaggerController(ISchemaRepository schemaRepository, IAppProvider appProvider,
            SchemasSwaggerGenerator schemasSwaggerGenerator)
        {
            this.appProvider = appProvider;

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
            var appEntity = await appProvider.FindAppByNameAsync(app);

            if (appEntity == null)
            {
                return NotFound();
            }

            var schemas = await schemaRepository.QueryAllAsync(appEntity.Id);

            var swaggerDocument = await schemasSwaggerGenerator.Generate(appEntity, schemas);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
