// ==========================================================================
//  FileCallbackResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Pipeline
{
    public class FileCallbackResult : FileResult
    {
        private readonly Func<Stream, Task> callback;

        public Func<Stream, Task> Callback
        {
            get { return callback; }
        }

        public FileCallbackResult(string contentType, string name, Func<Stream, Task> callback)
            : base(contentType)
        {
            FileDownloadName = name;

            this.callback = callback;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var executor = context.HttpContext.RequestServices.GetRequiredService<FileCallbackResultExecutor>();

            return executor.ExecuteAsync(context, this);
        }
    }
}

#pragma warning restore 1573