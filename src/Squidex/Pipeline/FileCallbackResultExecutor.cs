// ==========================================================================
//  FileCallbackResultExecutor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
                SetHeadersAndLog(context, result);

                await result.Callback(context.HttpContext.Response.Body);
            }
            catch
            {
                context.HttpContext.Response.Headers.Clear();
                context.HttpContext.Response.StatusCode = 404;
            }
        }
    }
}