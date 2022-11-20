// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Templates.Models;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Templates;

/// <summary>
/// Readonly API for news items.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Templates))]
public sealed class TemplatesController : ApiController
{
    private readonly TemplatesClient templatesClient;

    public TemplatesController(ICommandBus commandBus, TemplatesClient templatesClient)
        : base(commandBus)
    {
        this.templatesClient = templatesClient;
    }

    /// <summary>
    /// Get all templates.
    /// </summary>
    /// <response code="200">Templates returned.</response>.
    [HttpGet]
    [Route("templates/")]
    [ProducesResponseType(typeof(TemplatesDto), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await templatesClient.GetTemplatesAsync(HttpContext.RequestAborted);

        var response = TemplatesDto.FromDomain(templates, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Get template details.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <response code="200">Template returned.</response>.
    /// <response code="404">Template not found.</response>.
    [HttpGet]
    [Route("templates/{name}")]
    [ProducesResponseType(typeof(TemplateDetailsDto), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetTemplate(string name)
    {
        var details = await templatesClient.GetDetailAsync(name, HttpContext.RequestAborted);

        if (details == null)
        {
            return NotFound();
        }

        var response = TemplateDetailsDto.FromDomain(name, details, Resources);

        return Ok(response);
    }
}
