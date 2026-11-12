// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Areas.Api.Controllers;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Config;

public class AssetFileResolver(
    IAssetUsageTracker assetUsage,
    IUsageGate usageGate,
    IHttpClientFactory httpClientFactory,
    IOptions<AssetOptions> assetOptions)
{
    public async Task<IAssetFile> ToFileAsync(UploadModel model, HttpContext httpContext, App? app,
        CancellationToken ct)
    {
        Guard.NotNull(model);
        Guard.NotNull(httpContext);

        var file = await DownloadFileAsync(httpContext, ct) ?? GetFile(httpContext);

        if (app != null && !await IsSizeAllowedAsync(httpContext, app, file, ct))
        {
            await file.DisposeAsync();
            throw new ValidationException(T.Get("assets.maxSizeReached"));
        }

        return file;
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

        // Use the same limit as for direct uploads.
        var maxSize = assetOptions.Value.MaxSize > 0 ? assetOptions.Value.MaxSize : long.MaxValue;

        try
        {
            using var httpClient = httpClientFactory.CreateClient("Assets");

            // Only read the headers, otherwise the whole body is buffered in memory before the size is checked.
            using var httpResponse = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, ct);

            var length = httpResponse.Content.Headers.ContentLength;
            if (length == null || length > maxSize)
            {
                throw new ValidationException(T.Get("common.httpDownloadRequestSize"));
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new ValidationException(T.Get("common.httpDownloadFailed"));
            }

            await using var httpStream = await httpResponse.Content.ReadAsStreamAsync(ct);

            var tempFile = new TempAssetFile(fileName, httpResponse.Content.Headers.ContentType?.ToString()!);
            try
            {
                await using (var tempStream = tempFile.OpenWrite())
                {
                    await CopyWithLimitAsync(httpStream, tempStream, maxSize, ct);
                }
            }
            catch
            {
                await tempFile.DisposeAsync();
                throw;
            }

            return tempFile;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new ValidationException(T.Get("common.httpDownloadFailed"));
        }
    }

    private static async Task CopyWithLimitAsync(Stream source, Stream target, long maxSize,
        CancellationToken ct)
    {
        var buffer = new byte[81920];
        var bytesTotal = 0L;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, ct)) > 0)
        {
            bytesTotal += bytesRead;

            // Do not trust the content length header, the body could be longer.
            if (bytesTotal > maxSize)
            {
                throw new ValidationException(T.Get("common.httpDownloadRequestSize"));
            }

            await target.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
        }
    }

    private async Task<bool> IsSizeAllowedAsync(HttpContext httpContext, App app, IAssetFile file,
        CancellationToken ct)
    {
        var (plan, _, _) = await usageGate.GetPlanForAppAsync(app, true, ct);

        if (plan.MaxAssetSize <= 0)
        {
            return true;
        }

        var (_, currentSize) = await assetUsage.GetTotalByAppAsync(app.Id, ct);

        return plan.MaxAssetSize > currentSize + file.FileSize;
    }
}
