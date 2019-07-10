// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
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
        private readonly IOptions<MyContentsControllerOptions> controllerOptions;
        private readonly ITagService tagService;
        private readonly AssetOptions assetOptions;

        public AssetsController(
            ICommandBus commandBus,
            IAssetQueryService assetQuery,
            IAssetUsageTracker assetStatsRepository,
            IAppPlansProvider appPlansProvider,
            IOptions<AssetOptions> assetOptions,
            IOptions<MyContentsControllerOptions> controllerOptions,
            ITagService tagService)
            : base(commandBus)
        {
            this.assetOptions = assetOptions.Value;
            this.assetQuery = assetQuery;
            this.assetStatsRepository = assetStatsRepository;
            this.appPlansProvider = appPlansProvider;
            this.controllerOptions = controllerOptions;
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
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        [ApiPermission(Permissions.AppAssetsRead)]
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
        /// <param name="ids">The optional asset ids.</param>
        /// <returns>
        /// 200 => Assets returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all assets for the app.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiPermission(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssets(string app, [FromQuery] string ids = null)
        {
            var assets = await assetQuery.QueryAsync(Context, Q.Empty.WithODataQuery(Request.QueryString.ToString()).WithIds(ids));

            var response = Deferred.Response(() =>
            {
                return AssetsDto.FromAssets(assets, this, app);
            });

            if (controllerOptions.Value.EnableSurrogateKeys && assets.Count <= controllerOptions.Value.MaxItemsForSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = assets.ToSurrogateKeys();
            }

            Response.Headers[HeaderNames.ETag] = assets.ToEtag();

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
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiPermission(Permissions.AppAssetsRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAsset(string app, Guid id)
        {
            var asset = await assetQuery.FindAssetAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            var response = Deferred.Response(() =>
            {
                return AssetDto.FromAsset(asset, this, app);
            });

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = asset.ToSurrogateKey();
            }

            Response.Headers[HeaderNames.ETag] = asset.ToEtag();

            return Ok(response);
        }

        /// <summary>
        /// Upload a new asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 201 => Asset created.
        /// 404 => App not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        /// <remarks>
        /// You can only upload one file at a time. The mime type of the file is not calculated by Squidex and is required correctly.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [AssetRequestSizeLimit]
        [ApiPermission(Permissions.AppAssetsCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostAsset(string app, [SwaggerIgnore] List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new CreateAsset { File = assetFile };

            var response = await InvokeCommandAsync(app, command);

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
        /// 404 => Asset or app not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        /// <remarks>
        /// Use multipart request to upload an asset.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/content/")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetContent(string app, Guid id, [SwaggerIgnore] List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new UpdateAsset { File = assetFile, AssetId = id };

            var response = await InvokeCommandAsync(app, command);

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
        /// 400 => Asset name not valid.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/")]
        [ProducesResponseType(typeof(AssetDto), 200)]
        [AssetRequestSizeLimit]
        [ApiPermission(Permissions.AppAssetsUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAsset(string app, Guid id, [FromBody] AnnotateAssetDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Delete an asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset to delete.</param>
        /// <returns>
        /// 204 => Asset deleted.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/assets/{id}/")]
        [ApiPermission(Permissions.AppAssetsDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteAsset(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteAsset { AssetId = id });

            return NoContent();
        }

        private async Task<AssetDto> InvokeCommandAsync(string app, ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            if (context.PlainResult is AssetCreatedResult created)
            {
                return AssetDto.FromAsset(created.Asset, this, app, created.IsDuplicate);
            }
            else
            {
                return AssetDto.FromAsset(context.Result<IEnrichedAssetEntity>(), this, app);
            }
        }

        private async Task<AssetFile> CheckAssetFileAsync(IReadOnlyList<IFormFile> file)
        {
            if (file.Count != 1)
            {
                var error = new ValidationError($"Can only upload one file, found {file.Count} files.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var formFile = file[0];

            if (formFile.Length > assetOptions.MaxSize)
            {
                var error = new ValidationError($"File cannot be bigger than {assetOptions.MaxSize.ToReadableSize()}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var plan = appPlansProvider.GetPlanForApp(App);

            var currentSize = await assetStatsRepository.GetTotalSizeAsync(AppId);

            if (plan.MaxAssetSize > 0 && plan.MaxAssetSize < currentSize + formFile.Length)
            {
                var error = new ValidationError("You have reached your max asset size.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var assetFile = new AssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);

            return assetFile;
        }
    }
}
