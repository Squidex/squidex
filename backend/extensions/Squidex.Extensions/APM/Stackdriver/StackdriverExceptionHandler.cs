﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Google.Cloud.Diagnostics.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Extensions.APM.Stackdriver;

internal sealed class StackdriverExceptionHandler(IContextExceptionLogger logger, IHttpContextAccessor httpContextAccessor) : ILogAppender
{
    private readonly HttpContextWrapper httpContextWrapper = new HttpContextWrapper(httpContextAccessor);

    public sealed class HttpContextWrapper : IContextWrapper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        internal HttpContextWrapper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetHttpMethod()
        {
            return httpContextAccessor.HttpContext?.Request?.Method ?? string.Empty;
        }

        public string GetUri()
        {
            return httpContextAccessor.HttpContext?.Request?.GetDisplayUrl() ?? string.Empty;
        }

        public string GetUserAgent()
        {
            return httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString() ?? string.Empty;
        }
    }

    public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception? exception)
    {
        try
        {
            if (exception != null && exception is not DomainException && exception is not OperationCanceledException)
            {
                logger.Log(exception, httpContextWrapper);
            }
        }
        catch
        {
            return;
        }
    }
}
