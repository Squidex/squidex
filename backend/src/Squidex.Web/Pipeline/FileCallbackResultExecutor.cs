// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Squidex.Assets;

namespace Squidex.Web.Pipeline;

public sealed class FileCallbackResultExecutor : FileResultExecutorBase
{
    public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
        : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
    {
    }

    public async Task ExecuteAsync(ActionContext context, FileCallbackResult result)
    {
        var response = context.HttpContext.Response;

        // Always block execution of scripts and inline scripts for file downloads.
        response.Headers[HeaderNames.ContentSecurityPolicy] = "script-src 'none'";

        try
        {
            var (range, _, serveBody) = SetHeadersAndLog(context, result, result.FileSize, result.FileSize != null);

            if (!string.IsNullOrWhiteSpace(result.FileDownloadName) && result.SendInline)
            {
                var headerValue = new ContentDispositionHeaderValue("inline");

                headerValue.SetHttpFileName(result.FileDownloadName);

                // This produces a nice file name without downloading it, but executes the file and shows images directly.
                response.Headers[HeaderNames.ContentDisposition] = headerValue.ToString();
            }

            if (serveBody)
            {
                var bytesRange = new BytesRange(range?.From, range?.To);

                await result.Callback(response.Body, bytesRange, context.HttpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            if (!context.HttpContext.Response.HasStarted && result.ErrorAs404)
            {
                response.Headers.Clear();
                response.StatusCode = 404;

                Logger.LogCritical(new EventId(99), e, "Failed to send result.");
            }
            else
            {
                throw;
            }
        }
    }
}
