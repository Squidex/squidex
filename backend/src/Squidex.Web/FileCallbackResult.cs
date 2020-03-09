// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Web.Pipeline;

namespace Squidex.Web
{
    public sealed class FileCallbackResult : FileResult
    {
        public bool Send404 { get; set; }

        public bool SendInline { get; set; }

        public Func<Stream, Task> Callback { get; }

        public FileCallbackResult(string contentType, string? name, Func<Stream, Task> callback)
            : base(contentType)
        {
            Guard.NotNull(callback);

            FileDownloadName = name;

            Callback = callback;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var executor = context.HttpContext.RequestServices.GetRequiredService<FileCallbackResultExecutor>();

            return executor.ExecuteAsync(context, this);
        }
    }
}

#pragma warning restore 1573