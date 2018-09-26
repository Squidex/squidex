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
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Assets))]
    public sealed class AssetsController : ApiController
    {
        private readonly IAssetQueryService assetQuery;
        private readonly IAssetStatsRepository assetStatsRepository;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly IOptions<MyContentsControllerOptions> controllerOptions;
        private readonly ITagService tagService;
        private readonly AssetConfig assetsConfig;

        public AssetsController(
            ICommandBus commandBus,
            IAssetQueryService assetQuery,
            IAssetStatsRepository assetStatsRepository,
            IAppPlansProvider appPlanProvider,
            IOptions<AssetConfig> assetsConfig,
            IOptions<MyContentsControllerOptions> controllerOptions,
            ITagService tagService)
            : base(commandBus)
        {
            this.assetsConfig = assetsConfig.Value;
            this.assetQuery = assetQuery;
            this.assetStatsRepository = assetStatsRepository;
            this.appPlanProvider = appPlanProvider;
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
        [MustBeAppReader]
        [HttpGet]
        [Route("apps/{app}/assets/tags")]
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetTags(string app)
        {
            var response = await tagService.GetTagsAsync(App.Id, TagGroups.Assets);

            return Ok(response);
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
        [MustBeAppReader]
        [HttpGet]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssets(string app, [FromQuery] string ids = null)
        {
            var context = Context();

            var assets = await assetQuery.QueryAsync(context, Q.Empty.WithODataQuery(Request.QueryString.ToString()).WithIds(ids));

            var response = AssetsDto.FromAssets(assets);

            if (controllerOptions.Value.EnableSurrogateKeys && response.Items.Length <= controllerOptions.Value.MaxItemsForSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = response.Items.ToSurrogateKeys();
            }

            Response.Headers["ETag"] = response.Items.ToManyEtag(response.Total);

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
        [MustBeAppReader]
        [HttpGet]
        [Route("apps/{app}/assets/{id}/")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAsset(string app, Guid id)
        {
            var context = Context();

            var entity = await assetQuery.FindAssetAsync(context, id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = AssetDto.FromAsset(entity);

            if (controllerOptions.Value.EnableSurrogateKeys)
            {
                Response.Headers["Surrogate-Key"] = entity.Id.ToString();
            }

            Response.Headers["ETag"] = entity.Version.ToString();

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
        [MustBeAppEditor]
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostAsset(string app, [SwaggerIgnore] List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new CreateAsset { File = assetFile };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<AssetCreatedResult>();
            var response = AssetCreatedDto.FromCommand(command, result);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Replace asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 201 => Asset updated.
        /// 404 => Asset or app not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        /// <remarks>
        /// Use multipart request to upload an asset.
        /// </remarks>
        [MustBeAppEditor]
        [HttpPut]
        [Route("apps/{app}/assets/{id}/content/")]
        [ProducesResponseType(typeof(AssetReplacedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetContent(string app, Guid id, [SwaggerIgnore] List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new UpdateAsset { File = assetFile, AssetId = id };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<AssetSavedResult>();
            var response = AssetReplacedDto.Create(command, result);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The asset object that needs to updated.</param>
        /// <returns>
        /// 204 => Asset updated.
        /// 400 => Asset name not valid.
        /// 404 => Asset or app not found.
        /// </returns>
        [MustBeAppReader]
        [HttpPut]
        [Route("apps/{app}/assets/{id}/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAsset(string app, Guid id, [FromBody] AssetUpdateDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand(id));

            return NoContent();
        }

        /// <summary>
        /// Delete an asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset to delete.</param>
        /// <returns>
        /// 204 => Asset has been deleted.
        /// 404 => Asset or app not found.
        /// </returns>
        [MustBeAppEditor]
        [HttpDelete]
        [Route("apps/{app}/assets/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteAsset(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteAsset { AssetId = id });

            return NoContent();
        }

        private async Task<AssetFile> CheckAssetFileAsync(IReadOnlyList<IFormFile> file)
        {
            if (file.Count != 1)
            {
                var error = new ValidationError($"Can only upload one file, found {file.Count} files.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var formFile = file[0];

            if (formFile.Length > assetsConfig.MaxSize)
            {
                var error = new ValidationError($"File size cannot be longer than {assetsConfig.MaxSize.ToReadableSize()}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var plan = appPlanProvider.GetPlanForApp(App);

            var currentSize = await assetStatsRepository.GetTotalSizeAsync(App.Id);

            if (plan.MaxAssetSize > 0 && plan.MaxAssetSize < currentSize + formFile.Length)
            {
                var error = new ValidationError("You have reached your max asset size.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var assetFile = new AssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);

            return assetFile;
        }

        private QueryContext Context()
        {
            return QueryContext.Create(App, User);
        }
    }
}
