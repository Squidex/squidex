// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Assets))]
    public sealed class AssetsController : ApiController
    {
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetUsageTracker assetStatsRepository;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly ITagService tagService;

        public AssetsController(
            ICommandBus commandBus,
            IAssetQueryService assetQuery,
            IAssetUsageTracker assetStatsRepository,
            IAppPlansProvider appPlansProvider,
            ITagService tagService)
            : base(commandBus)
        {
            this.assetQuery = assetQuery;
            this.assetStatsRepository = assetStatsRepository;
            this.appPlansProvider = appPlansProvider;
            this.tagService = tagService;
        }

        /// <summary>
        /// Get assets tags.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Assets returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all tags for assets.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/assets/tags")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetTags(string app)
        {
            var tags = await tagService.GetTagsAsync(AppId, TagGroups.Assets);

            Response.Headers[HeaderNames.ETag] = tags.Version.ToString();

            return Ok(tags);
        }

        /// <summary>
        /// Get assets.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="parentId">The optional parent folder id.</param>
        /// <param name="ids">The optional asset ids.</param>
        /// <param name="q">The optional json query.</param>
        /// <returns>
        /// 200 => Assets returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all assets for the app.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssets(string app, [FromQuery] DomainId? parentId, [FromQuery] string? ids = null, [FromQuery] string? q = null)
        {
            var assets = await assetQuery.QueryAsync(Context, parentId, CreateQuery(ids, q));

            var response = Deferred.Response(() =>
            {
                return AssetsDto.FromAssets(assets, Resources);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get assets.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="query">The required query object.</param>
        /// <returns>
        /// 200 => Assets returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all assets for the app.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/assets/query")]
        [ProducesResponseType(typeof(AssetsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssetsPost(string app, [FromBody] QueryDto query)
        {
            var assets = await assetQuery.QueryAsync(Context, query?.ParentId, query?.ToQuery() ?? Q.Empty);

            var response = Deferred.Response(() =>
            {
                return AssetsDto.FromAssets(assets, Resources);
            });

            return Ok(response);
        }

        /// <summary>
        /// Get an asset by id.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset to retrieve.</param>
        /// <returns>
        /// 200 => Asset found.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/assets/{id}/")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAsset(string app, DomainId id)
        {
            var asset = await assetQuery.FindAsync(Context, id);

            if (asset == null)
            {
                return NotFound();
            }

            var response = Deferred.Response(() =>
            {
                return AssetDto.FromAsset(asset, Resources);
            });

            return Ok(response);
        }

        /// <summary>
        /// Upload a new asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="parentId">The optional parent folder id.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="id">The optional custom asset id.</param>
        /// <param name="duplicate">True to duplicate the asset, event if the file has been uploaded.</param>
        /// <returns>
        /// 201 => Asset created.
        /// 400 => Asset request not valid.
        /// 413 => Asset exceeds the maximum upload size.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// You can only upload one file at a time. The mime type of the file is not calculated by Squidex and is required correctly.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostAsset(string app, [FromQuery] DomainId parentId, IFormFile file, [FromQuery] DomainId? id = null, [FromQuery] bool duplicate = false)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new CreateAsset { File = assetFile, ParentId = parentId, Duplicate = duplicate };

            if (id != null && id.Value != default && !string.IsNullOrWhiteSpace(id.Value.ToString()))
            {
                command.AssetId = id.Value;
            }

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetAsset), new { app, id = response.Id }, response);
        }

        /// <summary>
        /// Replace asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 200 => Asset updated.
        /// 400 => Asset request not valid.
        /// 413 => Asset exceeds the maximum upload size.
        /// 404 => Asset or app not found.
        /// </returns>
        /// <remarks>
        /// Use multipart request to upload an asset.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/content/")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpload)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetContent(string app, DomainId id, IFormFile file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new UpdateAsset { File = assetFile, AssetId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The asset object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset updated.
        /// 400 => Asset request not valid.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAsset(string app, DomainId id, [FromBody] AnnotateAssetDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Moves the asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The asset object that needs to updated.</param>
        /// <returns>
        /// 200 => Asset moved.
        /// 400 => Asset request not valid.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/parent")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [AssetRequestSizeLimit]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetParent(string app, DomainId id, [FromBody] MoveAssetItemDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Delete an asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset to delete.</param>
        /// <param name="checkReferrers">True to check referrers of this asset.</param>
        /// <returns>
        /// 204 => Asset deleted.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/assets/{id}/")]
        [ApiPermissionOrAnonymous(Permissions.AppAssetsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteAsset(string app, DomainId id, [FromQuery] bool checkReferrers = false)
        {
            await CommandBus.PublishAsync(new DeleteAsset { AssetId = id, CheckReferrers = checkReferrers });

            return NoContent();
        }

        private async Task<AssetDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            if (context.PlainResult is AssetCreatedResult created)
            {
                return AssetDto.FromAsset(created.Asset, Resources, created.IsDuplicate);
            }
            else
            {
                return AssetDto.FromAsset(context.Result<IEnrichedAssetEntity>(), Resources);
            }
        }

        private async Task<AssetFile> CheckAssetFileAsync(IFormFile? file)
        {
            if (file == null || Request.Form.Files.Count != 1)
            {
                var error = T.Get("validation.onlyOneFile");

                throw new ValidationException(error);
            }

            var (plan, _) = appPlansProvider.GetPlanForApp(App);

            var currentSize = await assetStatsRepository.GetTotalSizeAsync(AppId);

            if (plan.MaxAssetSize > 0 && plan.MaxAssetSize < currentSize + file.Length)
            {
                var error = new ValidationError(T.Get("assets.maxSizeReached"));

                throw new ValidationException(error);
            }

            return file.ToAssetFile();
        }

        private Q CreateQuery(string? ids, string? q)
        {
            return Q.Empty
                .WithIds(ids)
                .WithJsonQuery(q)
                .WithODataQuery(Request.QueryString.ToString());
        }
    }
}
