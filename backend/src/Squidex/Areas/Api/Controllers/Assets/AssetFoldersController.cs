﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Commands;
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
        /// <returns>
        /// 200 => Asset folders returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all asset folders for the app.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/assets/folders", Order = -1)]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiPermission(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssetFolders(string app, [FromQuery] Guid parentId)
        {
            var assetFolders = await assetQuery.QueryAssetFoldersAsync(Context, parentId);

            var response = Deferred.Response(() =>
            {
                return AssetFoldersDto.FromAssets(assetFolders, this, app);
            });

            Response.Headers[HeaderNames.ETag] = assetFolders.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Upload a new asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The asset folder object that needs to be added to the App.</param>
        /// <returns>
        /// 201 => Asset folder created.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/assets/folders", Order = -1)]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [AssetRequestSizeLimit]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostAssetFolder(string app, [FromBody] CreateAssetFolderDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Updates the asset folder.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset folder.</param>
        /// <param name="request">The asset folder object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset folder updated.
        /// 400 => Asset folder name not valid.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [AssetRequestSizeLimit]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetFolder(string app, Guid id, [FromBody] RenameAssetFolderDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Moves the asset folder.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset folder.</param>
        /// <param name="request">The asset folder object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset folder moved.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/folders/{id}/parent", Order = -1)]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [AssetRequestSizeLimit]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetFolderParent(string app, Guid id, [FromBody] MoveAssetItemDto request)
        {
            var command = request.ToFolderCommand(id);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Delete an asset folder.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset folder to delete.</param>
        /// <returns>
        /// 204 => Asset folder deleted.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/assets/folders/{id}/", Order = -1)]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteAssetFolder(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteAssetFolder { AssetFolderId = id });

            return NoContent();
        }

        private async Task<AssetFolderDto> InvokeCommandAsync(string app, ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            return AssetFolderDto.FromAssetFolder(context.Result<IAssetFolderEntity>(), this, app);
        }
    }
}
