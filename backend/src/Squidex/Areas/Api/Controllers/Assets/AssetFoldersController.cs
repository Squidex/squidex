// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets;

/// <summary>
/// Uploads and retrieves assets.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Assets))]
public sealed class AssetFoldersController : ApiController
{
    private readonly IAssetQueryService assetQuery;

    public AssetFoldersController(ICommandBus commandBus, IAssetQueryService assetQuery)
        : base(commandBus)
    {
        this.assetQuery = assetQuery;
    }

    /// <summary>
    /// Get asset folders.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="parentId">The optional parent folder id.</param>
    /// <param name="scope">The scope of the query.</param>
    /// <response code="200">Asset folders returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Get all asset folders for the app.
    /// </remarks>
    [HttpGet]
    [Route("apps/{app}/assets/folders", Order = -1)]
    [ProducesResponseType(typeof(AssetFoldersDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAssetFolders(string app, [FromQuery] DomainId parentId, [FromQuery] AssetFolderScope scope = AssetFolderScope.PathAndItems)
    {
        var (folders, path) = await AsyncHelper.WhenAll(
            GetAssetFoldersAsync(parentId, scope),
            GetAssetPathAsync(parentId, scope));

        var response = Deferred.Response(() =>
        {
            return AssetFoldersDto.FromDomain(folders, path, Resources);
        });

        Response.Headers[HeaderNames.ETag] = folders.ToEtag();

        return Ok(response);
    }

    /// <summary>
    /// Create an asset folder.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The asset folder object that needs to be added to the App.</param>
    /// <response code="201">Asset folder created.</response>.
    /// <response code="400">Asset folder request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/assets/folders", Order = -1)]
    [ProducesResponseType(typeof(AssetFolderDto), StatusCodes.Status201Created)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostAssetFolder(string app, [FromBody] CreateAssetFolderDto request)
    {
        var command = request.ToCommand();

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetAssetFolders), new { parentId = request.ParentId, app }, response);
    }

    /// <summary>
    /// Update an asset folder.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset folder.</param>
    /// <param name="request">The asset folder object that needs to updated.</param>
    /// <response code="200">Asset folder updated.</response>.
    /// <response code="400">Asset folder request not valid.</response>.
    /// <response code="404">Asset folder or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
    [ProducesResponseType(typeof(AssetFolderDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutAssetFolder(string app, DomainId id, [FromBody] RenameAssetFolderDto request)
    {
        var command = request.ToCommand(id);

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Move an asset folder.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset folder.</param>
    /// <param name="request">The asset folder object that needs to updated.</param>
    /// <response code="200">Asset folder moved.</response>.
    /// <response code="400">Asset folder request not valid.</response>.
    /// <response code="404">Asset folder or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/folders/{id}/parent", Order = -1)]
    [ProducesResponseType(typeof(AssetFolderDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutAssetFolderParent(string app, DomainId id, [FromBody] MoveAssetFolderDto request)
    {
        var command = request.ToCommand(id);

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Delete an asset folder.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset folder to delete.</param>
    /// <response code="204">Asset folder deleted.</response>.
    /// <response code="404">Asset folder or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteAssetFolder(string app, DomainId id)
    {
        var command = new DeleteAssetFolder { AssetFolderId = id };

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    private async Task<AssetFolderDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return AssetFolderDto.FromDomain(context.Result<IAssetFolderEntity>(), Resources);
    }

    private Task<IReadOnlyList<IAssetFolderEntity>> GetAssetPathAsync(DomainId parentId, AssetFolderScope scope)
    {
        if (scope == AssetFolderScope.Items)
        {
            return Task.FromResult<IReadOnlyList<IAssetFolderEntity>>(ReadonlyList.Empty<IAssetFolderEntity>());
        }

        return assetQuery.FindAssetFolderAsync(Context.App.Id, parentId, HttpContext.RequestAborted);
    }

    private Task<IResultList<IAssetFolderEntity>> GetAssetFoldersAsync(DomainId parentId, AssetFolderScope scope)
    {
        if (scope == AssetFolderScope.Path)
        {
            return Task.FromResult(ResultList.Empty<IAssetFolderEntity>());
        }

        return assetQuery.QueryAssetFoldersAsync(Context, parentId, HttpContext.RequestAborted);
    }
}
