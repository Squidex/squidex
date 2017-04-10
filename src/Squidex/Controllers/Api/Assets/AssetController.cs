// ==========================================================================
//  AssetController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
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

#pragma warning disable 1573

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
        private readonly IAssetStore assetStorage;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly AssetConfig assetsConfig;

        public AssetController(
            ICommandBus commandBus, 
            IAssetStore assetStorage,
            IAssetRepository assetRepository,
            IAssetThumbnailGenerator assetThumbnailGenerator, 
            IOptions<AssetConfig> assetsConfig) 
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetsConfig = assetsConfig.Value;
            this.assetRepository = assetRepository;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get assets.
        /// </summary>
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
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 201 => Asset created.
        /// 404 => App not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/assets/")]
        [ProducesResponseType(typeof(AssetDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostAsset(string app, List<IFormFile> files)
        {
            if (files.Count != 1)
            {
                var error = new ValidationError($"Can only upload one file, found ${files.Count}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var file = files[0];

            if (file.Length > assetsConfig.MaxSize)
            {
                var error = new ValidationError($"File size cannot be longer than ${assetsConfig.MaxSize}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var fileContent = new MemoryStream();

            await file.OpenReadStream().CopyToAsync(fileContent);

            fileContent.Position = 0;

            var imageInfo = await assetThumbnailGenerator.GetImageInfoAsync(fileContent);

            var command = new CreateAsset
            {
                AssetId = Guid.NewGuid(),
                FileSize = file.Length,
                FileName = file.FileName,
                MimeType = file.ContentType,
                IsImage = imageInfo != null,
                PixelWidth = imageInfo?.PixelWidth,
                PixelHeight = imageInfo?.PixelHeight
            };
            
            fileContent.Position = 0;

            await assetStorage.UploadAssetAsync($"{command.AssetId}_0", fileContent);

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = AssetDto.Create(command, result);

            return StatusCode(201, response);
        }
    }
}
