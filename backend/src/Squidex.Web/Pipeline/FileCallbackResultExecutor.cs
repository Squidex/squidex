// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

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
                SetHeadersAndLog(context, result, null, false);

                if (!string.IsNullOrWhiteSpace(result.FileDownloadName) && result.SendInline)
                {
                    var headerValue = new ContentDispositionHeaderValue("inline");

                    headerValue.SetHttpFileName(result.FileDownloadName);

                    context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = headerValue.ToString();
                }

                await result.Callback(context.HttpContext.Response.Body);
            }
            catch (Exception e)
            {
                if (!context.HttpContext.Response.HasStarted && result.Send404)
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