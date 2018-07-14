// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Assets.Models;
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
        /// <param name="folderId">The folder to query from.</param>
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
        public async Task<IActionResult> GetAssets(string app, string folderId, [FromQuery] string ids = null)
        {
            var folderIdValue = ParseFolderId(folderId);

            var idsList = ParseIds(ids);

            var assets =
                idsList?.Count > 0 ?
                    await assetRepository.QueryAsync(App.Id, idsList) :
                    await assetRepository.QueryAsync(App.Id, folderIdValue, Request.QueryString.ToString());

            var response = AssetsDto.FromAssets(assets);

            Response.Headers["Surrogate-Key"] = string.Join(" ", response.Items.Where(x => !x.IsFolder).Select(x => x.Id));

            return Ok(response);
        }

        /// <summary>
        /// Get folder structure.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="path">The path to the folder to retrieve.</param>
        /// <returns>
        /// 200 => Folder found.
        /// 404 => Folder or app not found.
        /// </returns>
        [MustBeAppReader]
        [HttpGet]
        [Route("apps/{app}/assets/folder/{*path}")]
        [ProducesResponseType(typeof(FolderDto), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetFolder(string app, string path)
        {
            var idsList = ParseIds(path, '/');

            var folders = ResultList.Empty<IAssetEntity>();

            if (idsList?.Count > 0)
            {
                var foldersFromPath = await assetRepository.QueryAsync(App.Id, idsList);

                if (foldersFromPath.Count != idsList.Count)
                {
                    return NotFound();
                }

                for (var i = 0; i < idsList.Count - 1; i++)
                {
                    if (foldersFromPath[i + 1].FolderId != foldersFromPath[i + 1].Id)
                    {
                        return NotFound();
                    }
                }

                folders = foldersFromPath;
            }

            var assets = await assetRepository.QueryAsync(App.Id, folders.LastOrDefault()?.Id, Request.QueryString.ToString());

            var response = FolderDto.FromAssets(assets, folders.Select(FolderPathItem.FromAsset).ToArray());

            Response.Headers["Surrogate-Key"] = string.Join(" ", response.Items.Where(x => !x.IsFolder).Select(x => x.Id));

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
            var entity = await assetRepository.FindAssetAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = AssetDto.FromAsset(entity);

            Response.Headers["ETag"] = entity.Version.ToString();
            Response.Headers["Surrogate-Key"] = entity.Id.ToString();

            return Ok(response);
        }

        /// <summary>
        /// Upload asset or folder.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="folderId">The folder to upload the asset to.</param>
        /// <param name="request">The request if you want to create a folder.</param>
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
        public async Task<IActionResult> PostAsset(string app, string folderId, [SwaggerIgnore] List<IFormFile> file, CreateAssetFolderDto request)
        {
            Guid.TryParse(folderId, out var folderIdValue);

            if (file.Count > 0)
            {
                var assetFile = await CheckAssetFileAsync(file);

                var command = new CreateAsset { File = assetFile, FolderId = folderIdValue };
                var context = await CommandBus.PublishAsync(command);

                var result = context.Result<EntityCreatedResult<Guid>>();
                var response = AssetCreatedDto.FromCommand(command, result);

                return StatusCode(201, response);
            }
            else
            {
                var context = await CommandBus.PublishAsync(request.ToCommand(folderIdValue));

                var result = context.Result<EntityCreatedResult<Guid>>();
                var response = EntityCreatedDto.FromResult(result);

                return StatusCode(201, response);
            }
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
        public async Task<IActionResult> PutAsset(string app, Guid id, [FromBody] RenameAssetDto request)
        {
            await CommandBus.PublishAsync(request.ToCommand(id));

            return NoContent();
        }

        /// <summary>
        /// Moves the the asset.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="request">The asset object that needs to updated.</param>
        /// <returns>
        /// 204 => Asset moved.
        /// 400 => Asset folder not found.
        /// 404 => Asset or app not found.
        /// </returns>
        [MustBeAppReader]
        [HttpPut]
        [Route("apps/{app}/assets/{id}/move")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> MoveAsset(string app, Guid id, [FromBody] MoveAssetDto request)
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

        private static List<Guid> ParseIds(string ids, char separator = ',')
        {
            List<Guid> idsList = null;

            if (!string.IsNullOrWhiteSpace(ids))
            {
                idsList = new List<Guid>();

                foreach (var id in ids.Split(separator))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            return idsList;
        }

        private static Guid? ParseFolderId(string folderId)
        {
            Guid? parsedFolderId = null;

            if (!string.IsNullOrWhiteSpace(folderId))
            {
                if (Guid.TryParse(folderId, out var temp))
                {
                    parsedFolderId = temp;
                }
            }

            return parsedFolderId;
        }

        private async Task<AssetFile> CheckAssetFileAsync(IReadOnlyList<IFormFile> file)
        {
            if (file.Count > 1)
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
    }
}
