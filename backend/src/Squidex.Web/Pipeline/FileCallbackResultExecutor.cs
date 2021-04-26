// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Squidex.Assets;

namespace Squidex.Web.Pipeline
{
    public sealed class FileCallbackResultExecutor : FileResultExecutorBase
    {
        public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
            : base(CreateLogger<FileCallbackResultExecutor>(loggerFactory))
        {
        }

        public async Task ExecuteAsync(ActionContext context, FileCallbackResult result)
        {
            try
            {
                var (range, _, serveBody) = SetHeadersAndLog(context, result, result.FileSize, result.FileSize != null);

                if (!string.IsNullOrWhiteSpace(result.FileDownloadName) && result.SendInline)
                {
                    var headerValue = new ContentDispositionHeaderValue("inline");

                    headerValue.SetHttpFileName(result.FileDownloadName);

                    context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = headerValue.ToString();
                }

                if (serveBody)
                {
                    var bytesRange = new BytesRange(range?.From, range?.To);

                    await result.Callback(context.HttpContext.Response.Body, bytesRange, context.HttpContext.RequestAborted);
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
                    context.HttpContext.Response.Headers.Clear();
                    context.HttpContext.Response.StatusCode = 404;

                    Logger.LogCritical(new EventId(99), e, "Failed to send result.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}