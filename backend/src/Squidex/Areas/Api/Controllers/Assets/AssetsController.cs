// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Assets.Models;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets;

/// <summary>
/// Uploads and retrieves assets.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Assets))]
public sealed class AssetsController : ApiController
{
    private readonly IUsageGate usageGate;
    private readonly IAssetQueryService assetQuery;
    private readonly IAssetUsageTracker assetUsageTracker;
    private readonly ITagService tagService;
    private readonly AssetTusRunner assetTusRunner;

    public AssetsController(
        ICommandBus commandBus,
        IUsageGate usageGate,
        IAssetQueryService assetQuery,
        IAssetUsageTracker assetUsageTracker,
        ITagService tagService,
        AssetTusRunner assetTusRunner)
        : base(commandBus)
    {
        this.usageGate = usageGate;
        this.assetQuery = assetQuery;
        this.assetUsageTracker = assetUsageTracker;
        this.assetTusRunner = assetTusRunner;
        this.tagService = tagService;
    }

    /// <summary>
    /// Get assets tags.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Assets tags returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Get all tags for assets.
    /// </remarks>
    [HttpGet]
    [Route("apps/{app}/assets/tags")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetTags(string app)
    {
        var tags = await tagService.GetTagsAsync(AppId, TagGroups.Assets, HttpContext.RequestAborted);

        Response.Headers[HeaderNames.ETag] = tags.Version.ToString(CultureInfo.InvariantCulture);

        return Ok(tags);
    }

    /// <summary>
    /// Rename an asset tag.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="name">The tag to return.</param>
    /// <param name="request">The required request object.</param>
    /// <response code="200">Asset tag renamed and new tags returned.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/tags/{name}")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutTag(string app, string name, [FromBody] RenameTagDto request)
    {
        await tagService.RenameTagAsync(AppId, TagGroups.Assets, Uri.UnescapeDataString(name), request.TagName, HttpContext.RequestAborted);

        return await GetTags(app);
    }

    /// <summary>
    /// Get assets.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="parentId">The optional parent folder id.</param>
    /// <param name="ids">The optional asset ids.</param>
    /// <param name="q">The optional json query.</param>
    /// <response code="200">Assets returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Get all assets for the app.
    /// </remarks>
    [HttpGet]
    [Route("apps/{app}/assets/")]
    [ProducesResponseType(typeof(AssetsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAssets(string app, [FromQuery] DomainId? parentId, [FromQuery] string? ids = null, [FromQuery] string? q = null)
    {
        var assets = await assetQuery.QueryAsync(Context, parentId, CreateQuery(ids, q), HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return AssetsDto.FromDomain(assets, Resources);
        });

        return Ok(response);
    }

    /// <summary>
    /// Get assets.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="query">The required query object.</param>
    /// <response code="200">Assets returned.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Get all assets for the app.
    /// </remarks>
    [HttpPost]
    [Route("apps/{app}/assets/query")]
    [ProducesResponseType(typeof(AssetsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAssetsPost(string app, [FromBody] QueryDto query)
    {
        var assets = await assetQuery.QueryAsync(Context, query?.ParentId, query?.ToQuery() ?? Q.Empty, HttpContext.RequestAborted);

        var response = Deferred.Response(() =>
        {
            return AssetsDto.FromDomain(assets, Resources);
        });

        return Ok(response);
    }

    /// <summary>
    /// Get an asset by id.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset to retrieve.</param>
    /// <response code="200">Asset found.</response>.
    /// <response code="404">Asset or app not found.</response>.
    [HttpGet]
    [Route("apps/{app}/assets/{id}/")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(1)]
    public async Task<IActionResult> GetAsset(string app, DomainId id)
    {
        var asset = await assetQuery.FindAsync(Context, id, ct: HttpContext.RequestAborted);

        if (asset == null)
        {
            return NotFound();
        }

        var response = Deferred.Response(() =>
        {
            return AssetDto.FromDomain(asset, Resources);
        });

        return Ok(response);
    }

    /// <summary>
    /// Upload a new asset.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="201">Asset created.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="413">Asset exceeds the maximum upload size.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// You can only upload one file at a time. The mime type of the file is not calculated by Squidex and is required correctly.
    /// </remarks>
    [HttpPost]
    [Route("apps/{app}/assets/")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostAsset(string app, CreateAssetDto request)
    {
        var command = request.ToCommand(await CheckAssetFileAsync(request.File));

        var response = await InvokeCommandAsync(command);

        return CreatedAtAction(nameof(GetAsset), new { app, id = response.Id }, response);
    }

    /// <summary>
    /// Upload a new asset using tus.io.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="201">Asset created.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="413">Asset exceeds the maximum upload size.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// Use the tus protocol to upload an asset.
    /// </remarks>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("apps/{app}/assets/tus/{**fileId}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostAssetTus(string app)
    {
        var url = Url.Action(null, new { app, fileId = (object?)null })!;

        var (result, file) = await assetTusRunner.InvokeAsync(HttpContext, url);

        if (file != null)
        {
            var command = UpsertAssetDto.ToCommand(file);

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetAsset), new { app, id = response.Id }, response);
        }

        return result;
    }

    /// <summary>
    /// Bulk update assets.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The bulk update request.</param>
    /// <response code="200">Assets created, update or delete.</response>.
    /// <response code="400">Assets request not valid.</response>.
    /// <response code="404">App not found.</response>.
    [HttpPost]
    [Route("apps/{app}/assets/bulk")]
    [ProducesResponseType(typeof(BulkResultDto[]), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsRead)]
    [ApiCosts(5)]
    public async Task<IActionResult> BulkUpdateAssets(string app, [FromBody] BulkUpdateAssetsDto request)
    {
        var command = request.ToCommand();

        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var result = context.Result<BulkUpdateResult>();
        var response = result.Select(x => BulkResultDto.FromDomain(x, HttpContext)).ToArray();

        return Ok(response);
    }

    /// <summary>
    /// Upsert an asset.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The optional custom asset id.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="200">Asset created or updated.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="413">Asset exceeds the maximum upload size.</response>.
    /// <response code="404">App not found.</response>.
    /// <remarks>
    /// You can only upload one file at a time. The mime type of the file is not calculated by Squidex and is required correctly.
    /// </remarks>
    [HttpPost]
    [Route("apps/{app}/assets/{id}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsCreate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PostUpsertAsset(string app, DomainId id, UpsertAssetDto request)
    {
        var command = request.ToCommand(id, await CheckAssetFileAsync(request.File));

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Replace asset content.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset.</param>
    /// <param name="file">The file to upload.</param>
    /// <response code="200">Asset updated.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="413">Asset exceeds the maximum upload size.</response>.
    /// <response code="404">Asset or app not found.</response>.
    /// <remarks>
    /// Use multipart request to upload an asset.
    /// </remarks>
    [HttpPut]
    [Route("apps/{app}/assets/{id}/content/")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpload)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutAssetContent(string app, DomainId id, IFormFile file)
    {
        var command = new UpdateAsset { File = await CheckAssetFileAsync(file), AssetId = id };

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Update an asset.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset.</param>
    /// <param name="request">The asset object that needs to updated.</param>
    /// <response code="200">Asset updated.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="404">Asset or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/{id}/")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
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
    /// <param name="id">The ID of the asset.</param>
    /// <param name="request">The asset object that needs to updated.</param>
    /// <response code="200">Asset moved.</response>.
    /// <response code="400">Asset request not valid.</response>.
    /// <response code="404">Asset or app not found.</response>.
    [HttpPut]
    [Route("apps/{app}/assets/{id}/parent")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [AssetRequestSizeLimit]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsUpdate)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutAssetParent(string app, DomainId id, [FromBody] MoveAssetDto request)
    {
        var command = request.ToCommand(id);

        var response = await InvokeCommandAsync(command);

        return Ok(response);
    }

    /// <summary>
    /// Delete an asset.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the asset to delete.</param>
    /// <param name="request">The request parameters.</param>
    /// <response code="204">Asset deleted.</response>.
    /// <response code="404">Asset or app not found.</response>.
    [HttpDelete]
    [Route("apps/{app}/assets/{id}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppAssetsDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteAsset(string app, DomainId id, DeleteAssetDto request)
    {
        var command = request.ToCommand(id);

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpGet]
    [Route("apps/{app}/assets/completion")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetScriptCompletion(string app, string schema,
        [FromServices] ScriptingCompleter completer)
    {
        var completion = completer.AssetScript();

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/assets/completion/trigger")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetScriptTriggerCompletion(string app, string schema,
        [FromServices] ScriptingCompleter completer)
    {
        var completion = completer.AssetTrigger();

        return Ok(completion);
    }

    private async Task<AssetDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        if (context.PlainResult is AssetDuplicate created)
        {
            return AssetDto.FromDomain(created.Asset, Resources, true);
        }
        else
        {
            return AssetDto.FromDomain(context.Result<IEnrichedAssetEntity>(), Resources);
        }
    }

    private async Task<AssetFile> CheckAssetFileAsync(IFormFile? file)
    {
        if (file == null || Request.Form.Files.Count != 1)
        {
            var error = T.Get("validation.onlyOneFile");

            throw new ValidationException(error);
        }

        var (plan, _, _) = await usageGate.GetPlanForAppAsync(App, true, HttpContext.RequestAborted);

        var currentSize = await assetUsageTracker.GetTotalSizeByAppAsync(AppId, HttpContext.RequestAborted);

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
