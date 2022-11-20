// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.News.Models;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.News;

/// <summary>
/// Readonly API for news items.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(News))]
public sealed class NewsController : ApiController
{
    private readonly FeaturesService featuresService;

    public NewsController(ICommandBus commandBus, FeaturesService featuresService)
        : base(commandBus)
    {
        this.featuresService = featuresService;
    }

    /// <summary>
    /// Get features since version.
    /// </summary>
    /// <param name="version">The latest received version.</param>
    /// <response code="200">Latest features returned.</response>.
    [HttpGet]
    [Route("news/features/")]
    [ProducesResponseType(typeof(FeaturesDto), StatusCodes.Status200OK)]
    [ApiPermission]
    public async Task<IActionResult> GetNews([FromQuery] int version = 0)
    {
        var features = await featuresService.GetFeaturesAsync(version, HttpContext.RequestAborted);

        return Ok(features);
    }
}
