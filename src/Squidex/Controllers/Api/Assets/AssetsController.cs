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
        private readonly IAssetStorage assetStorage;
        private readonly AssetConfig assetsConfig;
        private readonly IAssetThumbnailGenerator thumbnailGenerator;

        public AssetsController(
            ICommandBus commandBus, 
            IAssetStorage assetStorage, 
            IAssetThumbnailGenerator thumbnailGenerator,
            IOptions<AssetConfig> assetsConfig) 
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetsConfig = assetsConfig.Value;
            this.thumbnailGenerator = thumbnailGenerator;
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

            var command = new CreateAsset
            {
                AssetId = Guid.NewGuid(),
                FileSize = file.Length,
                FileName = file.Name,
                MimeType = file.ContentType
            };
            
            var fileContent = new MemoryStream();

            await file.CopyToAsync(fileContent);

            fileContent.Position = 0;

            var fileThumbnail = await thumbnailGenerator.GetThumbnailOrNullAsync(fileContent, 200);

            if (fileThumbnail != null)
            {
                command.IsImage = true;

                await assetStorage.UploadAssetAsync(command.AssetId, fileThumbnail, "thumbnail");
            }

            fileContent.Position = 0;

            await assetStorage.UploadAssetAsync(command.AssetId, fileContent);

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>().IdOrValue;
            var response = new EntityCreatedDto { Id = result.ToString() };

            return StatusCode(201, response);
        }
    }
}
