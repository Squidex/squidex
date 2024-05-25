// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web;

public sealed class AssetFileModelBinder : IModelBinder
{
    private readonly IUsageGate usageGate;
    private readonly IAssetUsageTracker assetUsage;
    private readonly IHttpClientFactory httpClientFactory;

    public AssetFileModelBinder(IUsageGate usageGate, IAssetUsageTracker assetUsage, IHttpClientFactory httpClientFactory)
    {
        this.usageGate = usageGate;
        this.assetUsage = assetUsage;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var httpContext = bindingContext.ActionContext.HttpContext;

        var file = await DownloadFileAsync(httpContext, httpContext.RequestAborted) ?? GetFile(httpContext);

        if (!await IsSizeAllowedAsync(httpContext, file.FileSize, httpContext.RequestAborted))
        {
            await file.DisposeAsync();
            throw new ValidationException(T.Get("assets.maxSizeReached"));
        }

        bindingContext.Result = ModelBindingResult.Success(file);
    }

    private static IAssetFile GetFile(HttpContext httpContext)
    {
        var requestFiles = httpContext.Request.Form.Files;

        if (requestFiles.Count != 1)
        {
            throw new ValidationException(T.Get("validation.onlyOneFile"));
        }

        var formFile = requestFiles[0];

        if (string.IsNullOrWhiteSpace(formFile.ContentType))
        {
            throw new ValidationException(T.Get("common.httpContentTypeNotDefined"));
        }

        if (string.IsNullOrWhiteSpace(formFile.FileName))
        {
            throw new ValidationException(T.Get("common.httpFileNameNotDefined"));
        }

        return new DelegateAssetFile(
            formFile.FileName,
            formFile.ContentType,
            formFile.Length,
            formFile.OpenReadStream);
    }

    private async Task<IAssetFile?> DownloadFileAsync(HttpContext httpContext,
        CancellationToken ct)
    {
        if (httpContext.Request.Form.Files.Count > 0)
        {
            return null;
        }

        var fileUrl = httpContext.Request.Form["url"].ToString();
        var fileName = httpContext.Request.Form["name"].ToString();

        if (string.IsNullOrEmpty(fileUrl) ||
            string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var requestSize = httpContext.Features.Get<IRequestSizeLimitMetadata>()?.MaxRequestBodySize ?? int.MaxValue;

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            using var httpResponse = await httpClient.GetAsync(fileUrl, ct);

            var length = httpResponse.Content.Headers.ContentLength;
            if (length == null || length > requestSize)
            {
                throw new ValidationException(T.Get("common.httpDownloadRequestSize"));
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new ValidationException(T.Get("common.httpDownloadFailed"));
            }

            await using var httpStream = await httpResponse.Content.ReadAsStreamAsync(ct);

            var tempFile = new TempAssetFile(fileName, httpResponse.Content.Headers.ContentType?.ToString()!);

            await using (var tempStream = tempFile.OpenWrite())
            {
                await httpStream.CopyToAsync(tempStream, ct);
            }

            return tempFile;
        }
        catch
        {
            throw new ValidationException(T.Get("common.httpDownloadFailed"));
        }
    }

    private async Task<bool> IsSizeAllowedAsync(HttpContext httpContext, long size,
        CancellationToken ct)
    {
        if (httpContext.Features.Get<App>() is not App app)
        {
            return true;
        }

        var (plan, _, _) = await usageGate.GetPlanForAppAsync(app, true, ct);

        if (plan.MaxAssetSize <= 0)
        {
            return true;
        }

        var (_, currentSize) = await assetUsage.GetTotalByAppAsync(app.Id, ct);

        return plan.MaxAssetSize < currentSize + size;
    }
}
