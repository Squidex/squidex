// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Web.Pipeline;

namespace Squidex.Web
{
    public sealed class FileCallbackResult : FileResult
    {
        public bool ErrorAs404 { get; set; }

        public bool SendInline { get; set; }

        public long? FileSize { get; set; }

        public Func<Stream, BytesRange, CancellationToken, Task> Callback { get; }

        public FileCallbackResult(string contentType, Func<Stream, CancellationToken, Task> callback)
            : base(contentType)
        {
            Guard.NotNull(callback);

            Callback = (stream, _, ct) => callback(stream, ct);
        }

        public FileCallbackResult(string contentType, Func<Stream, BytesRange, CancellationToken, Task> callback)
            : base(contentType)
        {
            Guard.NotNull(callback);

            Callback = callback;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            var executor = context.HttpContext.RequestServices.GetRequiredService<FileCallbackResultExecutor>();

            return executor.ExecuteAsync(context, this);
        }
    }
}