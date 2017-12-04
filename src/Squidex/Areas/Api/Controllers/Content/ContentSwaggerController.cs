﻿// ==========================================================================
//  ContentSwaggerController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Domain.Apps.Read;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentSwaggerController : ApiController
    {
        private readonly IAppProvider appProvider;
        private readonly SchemasSwaggerGenerator schemasSwaggerGenerator;

        public ContentSwaggerController(ICommandBus commandBus, IAppProvider appProvider, SchemasSwaggerGenerator schemasSwaggerGenerator)
            : base(commandBus)
        {
            this.appProvider = appProvider;

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
            var schemas = await appProvider.GetSchemasAsync(AppName);

            var swaggerDocument = await schemasSwaggerGenerator.Generate(App, schemas);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
