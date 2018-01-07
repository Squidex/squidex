// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;

namespace Squidex.Pipeline
{
    public sealed class FileCallbackResultExecutor : FileResultExecutorBase
    {
        public FileCallbackResultExecutor(ILoggerFactory loggerFactory)
            : base(CreateLogger<VirtualFileResultExecutor>(loggerFactory))
        {
        }

        public async Task ExecuteAsync(ActionContext context, FileCallbackResult result)
        {
            try
            {
                SetHeadersAndLog(context, result, null);

                await result.Callback(context.HttpContext.Response.Body);
            }
            catch (Exception e)
            {
                Logger.LogCritical(new EventId(99), e, "Failed to send result.");

                context.HttpContext.Response.Headers.Clear();
                context.HttpContext.Response.StatusCode = 404;
            }
        }
    }
}