// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents;

public sealed class ContentOpenApiController : ApiController
{
    private readonly IAppProvider appProvider;
    private readonly SchemasOpenApiGenerator schemasOpenApiGenerator;

    public ContentOpenApiController(ICommandBus commandBus, IAppProvider appProvider,
        SchemasOpenApiGenerator schemasOpenApiGenerator)
        : base(commandBus)
    {
        this.appProvider = appProvider;
        this.schemasOpenApiGenerator = schemasOpenApiGenerator;
    }

    [HttpGet]
    [Route("content/{app}/docs/")]
    [ApiCosts(0)]
    [AllowAnonymous]
    public IActionResult Docs(string app)
    {
        var vm = new DocsVM
        {
            Specification = $"~/api/content/{app}/swagger/v1/swagger.json"
        };

        return View(nameof(Docs), vm);
    }

    [HttpGet]
    [Route("content/{app}/docs/flat/")]
    [ApiCosts(0)]
    [AllowAnonymous]
    public IActionResult DocsFlat(string app)
    {
        var vm = new DocsVM
        {
            Specification = $"~/api/content/{app}/flat/swagger/v1/swagger.json"
        };

        return View(nameof(Docs), vm);
    }

    [HttpGet]
    [Route("content/{app}/swagger/v1/swagger.json")]
    [ApiCosts(0)]
    [AllowAnonymous]
    public async Task<IActionResult> GetOpenApi(string app)
    {
        var schemas = await appProvider.GetSchemasAsync(AppId, HttpContext.RequestAborted);

        var openApiDocument = await schemasOpenApiGenerator.GenerateAsync(HttpContext, App, schemas, false);

        return Content(openApiDocument.ToJson(), "application/json");
    }

    [HttpGet]
    [Route("content/{app}/flat/swagger/v1/swagger.json")]
    [ApiCosts(0)]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlatOpenApi(string app)
    {
        var schemas = await appProvider.GetSchemasAsync(AppId, HttpContext.RequestAborted);

        var openApiDocument = await schemasOpenApiGenerator.GenerateAsync(HttpContext, App, schemas, true);

        return Content(openApiDocument.ToJson(), "application/json");
    }
}
