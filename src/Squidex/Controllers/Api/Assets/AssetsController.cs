// ==========================================================================
//  AssetsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.Api.Assets.Models;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag("Assets")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetRepository assetRepository;
        private readonly IAssetStatsRepository assetStatsRepository;
        private readonly IAppPlansProvider appPlanProvider;
        private readonly AssetConfig assetsConfig;

        public AssetsController(
            ICommandBus commandBus, 
            IAssetRepository assetRepository,
            IAssetStatsRepository assetStatsRepository,
            IAppPlansProvider appPlanProvider,
            IOptions<AssetConfig> assetsConfig) 
            : base(commandBus)
        {
            this.assetsConfig = assetsConfig.Value;
            this.assetRepository = assetRepository;
            this.assetStatsRepository = assetStatsRepository;
            this.appPlanProvider = appPlanProvider;
        }

        /// <summary>
        /// Get assets.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="ids">The optional asset ids.</param>
        /// <param name="skip">The number of assets to skip.</param>
        /// <param name="take">The number of assets to take (Default: 20).</param>
        /// <param name="query">The query to limit the files by name.</param>
        /// <param name="mimeTypes">Comma separated list of mime types to get.</param>
        /// <returns>
        /// 200 => Assets returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Get all assets for the app. Mime types can be comma-separated, e.g. application/json,text/html.
        /// </remarks>
        [MustBeAppReader]
        [HttpGet]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAssets(string app, [FromQuery] string query = null, [FromQuery] string mimeTypes = null, [FromQuery] string ids = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var mimeTypeList = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(mimeTypes))
            {
                foreach (var mimeType in mimeTypes.Split(','))
                {
                    mimeTypeList.Add(mimeType.Trim());
                }
            }

            var idsList = new HashSet<Guid>();

            if (!string.IsNullOrWhiteSpace(ids))
            {
                foreach (var id in ids.Split(','))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            var taskForItems = assetRepository.QueryAsync(AppId, mimeTypeList, idsList, query, take, skip);
            var taskForCount = assetRepository.CountAsync(AppId, mimeTypeList, idsList, query);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new AssetsDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Select(x => SimpleMapper.Map(x, new AssetDto { FileType = x.FileName.FileType() })).ToArray()
            };

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
        [Route("apps/{app}/assets/{id}")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetAsset(string app, Guid id)
        {
            var entity = await assetRepository.FindAssetAsync(id);

            if (entity == null || entity.IsDeleted)
            {
                return NotFound();
            }

            var response = SimpleMapper.Map(entity, new AssetDto { FileType = entity.FileName.FileType() });

            Response.Headers["ETag"] = new StringValues(entity.Version.ToString());

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
        /// You can only upload one file at a time. The mime type of the file is not calculated by Squidex and must be defined correctly.
        /// </remarks>
        [MustBeAppEditor]
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostAsset(string app, List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new CreateAsset { File = assetFile };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = AssetCreatedDto.Create(command, result);

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
        [MustBeAppEditor]
        [HttpPut]
        [Route("apps/{app}/assets/{id}/content")]
        [ProducesResponseType(typeof(AssetReplacedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAssetContent(string app, Guid id, List<IFormFile> file)
        {
            var assetFile = await CheckAssetFileAsync(file);

            var command = new UpdateAsset { File = assetFile, AssetId = id };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntitySavedResult>();
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
        [Route("apps/{app}/assets/{id}")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutAsset(string app, Guid id, [FromBody] AssetUpdateDto request)
        {
            var command = SimpleMapper.Map(request, new RenameAsset { AssetId = id });

            await CommandBus.PublishAsync(command);

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
                var error = new ValidationError($"Can only upload one file, found {file.Count}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var formFile = file[0];

            if (formFile.Length > assetsConfig.MaxSize)
            {
                var error = new ValidationError($"File size cannot be longer than ${assetsConfig.MaxSize}.");

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
    }
}
