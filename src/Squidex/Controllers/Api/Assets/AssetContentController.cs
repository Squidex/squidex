// ==========================================================================
//  AssetContentController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;
using Squidex.Read.Assets.Repositories;

#pragma warning disable 1573

namespace Squidex.Controllers.Api.Assets
{
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerIgnore]
    public class AssetContentController : ControllerBase
    {
        private readonly IAssetStore assetStorage;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetStore assetStorage,
            IAssetRepository assetRepository,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetRepository = assetRepository;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        [HttpGet]
        [Route("assets/{id}/")]
        public async Task<IActionResult> GetAssetContent(string app, Guid id, [FromQuery] int? width = null, [FromQuery] int? height = null, [FromQuery] string mode = null)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            Stream content;

            if (asset.IsImage && (width.HasValue || height.HasValue))
            {
                var suffix = $"{width}_{height}_{mode}";

                content = await assetStorage.GetAssetAsync(asset.Id, asset.Version, suffix);

                if (content == null)
                {
                    var fullSizeContent = await assetStorage.GetAssetAsync(asset.Id, asset.Version);

                    if (fullSizeContent == null)
                    {
                        return NotFound();
                    }

                    content = await assetThumbnailGenerator.CreateThumbnailAsync(fullSizeContent, width, height, mode);

                    await assetStorage.UploadAssetAsync(asset.Id, asset.Version, content, suffix);

                    content.Position = 0;
                }
            }
            else
            {
                content = await assetStorage.GetAssetAsync(asset.Id, asset.Version);
            }

            if (content == null)
            {
                return NotFound();
            }

            return new FileStreamResult(content, asset.MimeType);
        }
    }
}
