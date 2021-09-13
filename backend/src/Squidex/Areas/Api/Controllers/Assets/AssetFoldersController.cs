// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

namespace Squidex.Areas.Api.Controllers.Assets
{
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
        /// <returns>
        /// 200 => Asset folders returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all asset folders for the app.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/assets/folders", Order = -1)]
        [ProducesResponseType(typeof(AssetFoldersDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssetFolders(string app, [FromQuery] DomainId parentId, [FromQuery] AssetFolderScope scope = AssetFolderScope.PathAndItems)
        {
            var (folders, path) = await AsyncHelper.WhenAll(
                GetAssetFoldersAsync(parentId, scope),
                GetAssetPathAsync(parentId, scope));

            var response = Deferred.Response(() =>
            {
                return AssetFoldersDto.FromAssets(folders, path, Resources);
            });

            Response.Headers[HeaderNames.ETag] = folders.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Create an asset folder.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The asset folder object that needs to be added to the App.</param>
        /// <returns>
        /// 201 => Asset folder created.
        /// 400 => Asset folder request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/assets/folders", Order = -1)]
        [ProducesResponseType(typeof(AssetFolderDto), 201)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
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
        /// <param name="id">The id of the asset folder.</param>
        /// <param name="request">The asset folder object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset folder updated.
        /// 400 => Asset folder request not valid.
        /// 404 => Asset folder or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
        [ProducesResponseType(typeof(AssetFolderDto), StatusCodes.Status200OK)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
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
        /// <param name="id">The id of the asset folder.</param>
        /// <param name="request">The asset folder object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset folder moved.
        /// 400 => Asset folder request not valid.
        /// 404 => Asset folder or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/folders/{id}/parent", Order = -1)]
        [ProducesResponseType(typeof(AssetFolderDto), StatusCodes.Status200OK)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
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
        /// <param name="id">The id of the asset folder to delete.</param>
        /// <returns>
        /// 204 => Asset folder deleted.
        /// 404 => Asset folder or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteAssetFolder(string app, DomainId id)
        {
            await CommandBus.PublishAsync(new DeleteAssetFolder { AssetFolderId = id });

            return NoContent();
        }

        private async Task<AssetFolderDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            return AssetFolderDto.FromAssetFolder(context.Result<IAssetFolderEntity>(), Resources);
        }

        private Task<IReadOnlyList<IAssetFolderEntity>> GetAssetPathAsync(DomainId parentId, AssetFolderScope scope)
        {
            if (scope == AssetFolderScope.Items)
            {
                return Task.FromResult<IReadOnlyList<IAssetFolderEntity>>(ImmutableList.Empty<IAssetFolderEntity>());
            }

            return assetQuery.FindAssetFolderAsync(Context.App.Id, parentId, HttpContext.RequestAborted);
        }

        private Task<IResultList<IAssetFolderEntity>> GetAssetFoldersAsync(DomainId parentId, AssetFolderScope scope)
        {
            if (scope == AssetFolderScope.Path)
            {
                return Task.FromResult(ResultList.CreateFrom<IAssetFolderEntity>(0));
            }

            return assetQuery.QueryAssetFoldersAsync(Context, parentId, HttpContext.RequestAborted);
        }
    }
}
