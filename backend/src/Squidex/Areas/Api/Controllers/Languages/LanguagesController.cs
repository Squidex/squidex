// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Languages;

/// <summary>
/// Readonly API for supported languages.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Languages))]
public sealed class LanguagesController : ApiController
{
    public LanguagesController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    /// <summary>
    /// Get supported languages.
    /// </summary>
    /// <remarks>
    /// Provide a list of supported language codes, following the ISO2Code standard.
    /// </remarks>
    /// <response code="200">Supported language codes returned.</response>.
    [HttpGet]
    [Route("languages/")]
    [ProducesResponseType(typeof(LanguageDto[]), StatusCodes.Status200OK)]
    [ApiPermission]
    public IActionResult GetLanguages()
    {
        var response = Deferred.Response(() =>
        {
            return Language.AllLanguages.Select(LanguageDto.FromDomain).ToArray();
        });

        Response.Headers[HeaderNames.ETag] = "1";

        return Ok(response);
    }
}
