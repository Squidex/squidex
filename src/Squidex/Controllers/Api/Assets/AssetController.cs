// ==========================================================================
//  AssetController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Controllers.Api.Assets.Models;
using Squidex.Core.Identity;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Assets.Repositories;
using Squidex.Write.Assets.Commands;

namespace Squidex.Controllers.Api.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [Authorize(Roles = SquidexRoles.AppEditor)]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Assets")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetRepository assetRepository;
        private readonly AssetConfig assetsConfig;

        public AssetController(
            ICommandBus commandBus, 
            IAssetRepository assetRepository,
            IOptions<AssetConfig> assetsConfig) 
            : base(commandBus)
        {
            this.assetsConfig = assetsConfig.Value;
            this.assetRepository = assetRepository;
        }

        /// <summary>
        /// Get assets.
        /// </summary>
        /// <param name="skip">The number of assets to skip.</param>
        /// <param name="take">The number of assets to take.</param>
        /// <param name="query">The query to limit the files by name.</param>
        /// <param name="mimeTypes">Comma separated list of mime types to get.</param>
        /// <returns>
        /// 200 => assets returned.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetsDto), 200)]
        public async Task<IActionResult> GetAssets([FromQuery] string query = null, [FromQuery] string mimeTypes = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var mimeTypeList = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(mimeTypes))
            {
                foreach (var mimeType in mimeTypes.Split(','))
                {
                    mimeTypeList.Add(mimeType.Trim());
                }
            }

            var taskForAssets = assetRepository.QueryAsync(AppId, mimeTypeList, query, take, skip);
            var taskForCount = assetRepository.CountAsync(AppId, mimeTypeList, query);

            await Task.WhenAll(taskForAssets, taskForCount);

            var model = new AssetsDto
            {
                Total = taskForCount.Result,
                Items = taskForAssets.Result.Select(x => SimpleMapper.Map(x, new AssetDto())).ToArray()
            };

            return Ok(model);
        }

        /// <summary>
        /// Creates and uploads a new asset.
        /// </summary>
        /// <param name="app">The app where the asset is a part of.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 201 => Asset created.
        /// 404 => App not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostAsset(string app, List<IFormFile> file)
        {
            var assetFile = GetAssetFile(file);

            var command = new CreateAsset { File = assetFile };
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = AssetDto.Create(command, result);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Replaces the content of the asset with a newer version.
        /// </summary>
        /// <param name="app">The app where the asset is a part of.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>
        /// 201 => Asset updated.
        /// 404 => App or Asset not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/assets/{id}/content")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PutAssetContent(string app, Guid id, List<IFormFile> file)
        {
            var assetFile = GetAssetFile(file);
            
            await CommandBus.PublishAsync(new UpdateAsset { File = assetFile });

            return NoContent();
        }

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="app">The app where the asset is a part of.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The asset object that needs to updated.</param>
        /// <returns>
        /// 201 => Asset updated.
        /// 404 => App or Asset not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/assets/{id}/content")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PutAsset(string app, Guid id, [FromBody]  AssetUpdateDto request)
        {
            var command = SimpleMapper.Map(request, new RenameAsset());

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete an asset.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="id">The id of the asset to delete.</param>
        /// <returns>
        /// 204 => Asset has been deleted.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/")]
        public async Task<IActionResult> DeleteSchema(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteAsset { AssetId = id });

            return NoContent();
        }

        private AssetFile GetAssetFile(IReadOnlyList<IFormFile> file)
        {
            if (file.Count != 1)
            {
                var error = new ValidationError($"Can only upload one file, found ${file.Count}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var formFile = file[0];

            if (formFile.Length > assetsConfig.MaxSize)
            {
                var error = new ValidationError($"File size cannot be longer than ${assetsConfig.MaxSize}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var assetFile = new AssetFile(formFile.FileName, formFile.ContentType, formFile.Length, formFile.OpenReadStream);

            return assetFile;
        }
    }
}
