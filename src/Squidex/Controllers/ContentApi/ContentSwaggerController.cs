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
using Squidex.Pipeline;
using Squidex.Read.Apps.Services;
using Squidex.Read.Schemas.Repositories;

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

        public ContentSwaggerController(ISchemaRepository schemaRepository, IAppProvider appProvider, SchemasSwaggerGenerator schemasSwaggerGenerator)
        {
            this.appProvider = appProvider;

            this.schemaRepository = schemaRepository;
            this.schemasSwaggerGenerator = schemasSwaggerGenerator;
        }

        [HttpGet]
        [Route("content/{app}/docs/")]
        public IActionResult Docs(string app)
        {
            ViewBag.Specification = $"~/content/{app}/swagger/v1/swagger.json";

            return View("Docs");
        }

        [HttpGet]
        [Route("content/{app}/swagger/v1/swagger.json")]
        public async Task<IActionResult> GetSwagger(string app)
        {
            var appEntity = await appProvider.FindAppByNameAsync(app);

            if (appEntity == null)
            {
                return NotFound();
            }
           
            var schemas = await schemaRepository.QueryAllWithSchemaAsync(appEntity.Id);
            
            var swaggerDocument = await schemasSwaggerGenerator.Generate(appEntity, schemas);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
