// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Contents
{
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
        [AllowAnonymous]
        public IActionResult Docs(string app)
        {
            var vm = new DocsVM { Specification = $"~/content/{app}/swagger/v1/swagger.json" };

            return View(nameof(Docs), vm);
        }

        [HttpGet]
        [Route("content/{app}/swagger/v1/swagger.json")]
        [ApiCosts(0)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSwagger(string app)
        {
            var schemas = await appProvider.GetSchemasAsync(AppId);

            var swaggerDocument = await schemasSwaggerGenerator.Generate(HttpContext, App, schemas);

            return Content(swaggerDocument.ToJson(), "application/json");
        }
    }
}
