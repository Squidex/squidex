// ==========================================================================
//  AssetsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Core.Identity;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Assets;
using Squidex.Pipeline;
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
    public class AssetsController : ControllerBase
    {
        private readonly IAssetStore assetStorage;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly AssetConfig assetsConfig;

        public AssetsController(
            ICommandBus commandBus, 
            IAssetStore assetStorage, 
            IAssetThumbnailGenerator assetThumbnailGenerator, 
            IOptions<AssetConfig> assetsConfig) 
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.assetsConfig = assetsConfig.Value;
        }

        /// <summary>
        /// Creates and uploads a new asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="file">The name of the schema.</param>
        /// <returns>
        /// 201 => Asset created.
        /// 404 => App not found.
        /// 400 => Asset exceeds the maximum size.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/{name}/fields/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public async Task<IActionResult> PostAsset(string app, IFormFile file)
        {
            if (file.Length > assetsConfig.MaxSize)
            {
                var error = new ValidationError($"File size cannot be longer than ${assetsConfig.MaxSize}.");

                throw new ValidationException("Cannot create asset.", error);
            }

            var fileContent = new MemoryStream();

            await file.OpenReadStream().CopyToAsync(fileContent);

            fileContent.Position = 0;

            var command = new CreateAsset
            {
                AssetId = Guid.NewGuid(),
                FileSize = file.Length,
                FileName = file.Name,
                MimeType = file.ContentType,
                IsImage = await assetThumbnailGenerator.IsValidImageAsync(fileContent)
            };
            
            fileContent.Position = 0;

            await assetStorage.UploadAssetAsync($"{command.AssetId}_0", fileContent);

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>().IdOrValue;
            var response = new EntityCreatedDto { Id = result.ToString() };

            return StatusCode(201, response);
        }
    }
}
