// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Apps.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps;

/// <summary>
/// Update and query apps.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Apps))]
public sealed class AppLanguagesController : ApiController
{
    public AppLanguagesController(ICommandBus commandBus)
        : base(commandBus)
    {
    }

    /// <summary>
    /// Get app languages.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Languages returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpGet]
    [Route("apps/{app}/languages/")]
    [ProducesResponseType(typeof(AppLanguagesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppLanguagesRead)]
    [ApiCosts(0)]
    public IActionResult GetLanguages(string app)
    {
        var response = Deferred.Response(() =>
        {
            return GetResponse(App);
        });

        Response.Headers[HeaderNames.ETag] = App.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Attaches an app language.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The language to add to the app.</param>
    /// <response code="201">Language created.</response>.
    /// <response code="400">Language request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/languages/")]
    [ProducesResponseType(typeof(AppLanguagesDto), 201)]
    [ApiPermissionOrAnonymous(PermissionIds.AppLanguagesCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostLanguage(string app, [FromBody] AddLanguageDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetLanguages), new { app }, response);
    }

    /// <summary>
    /// Updates an app language.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="language">The language to update.</param>
    /// <param name="request">The language object.</param>
    /// <response code="200">Language updated.</response>.
    /// <response code="400">Language request not valid.</response>.
    /// <response code="404">Language or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/languages/{language}/")]
    [ProducesResponseType(typeof(AppLanguagesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppLanguagesUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutLanguage(string app, string language, [FromBody] UpdateLanguageDto request)
    {
        var command = request.ToCommand(ParseLanguage(language));

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Deletes an app language.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="language">The language to delete from the app.</param>
    /// <response code="200">Language deleted.</response>.
    /// <response code="400">Language is master language.</response>.
    /// <response code="404">Language or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/languages/{language}/")]
    [ProducesResponseType(typeof(AppLanguagesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppLanguagesDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteLanguage(string app, string language)
    {
        var command = new RemoveLanguage { Language = ParseLanguage(language) };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    private async Task<AppLanguagesDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<IAppEntity>();
        var response = GetResponse(result);

        return response;
    }

    private AppLanguagesDto GetResponse(IAppEntity result)
    {
        return AppLanguagesDto.FromDomain(result, Resources);
    }

    private static Language ParseLanguage(string language)
    {
        try
        {
            return Language.GetLanguage(language);
        }
        catch (NotSupportedException)
        {
            throw new DomainObjectNotFoundException(language);
        }
    }
}
