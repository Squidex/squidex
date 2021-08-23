// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Google.Cloud.Diagnostics.AspNetCore;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Extensions.APM.Stackdriver
{
    internal class StackdriverExceptionHandler : ILogAppender
    {
        private readonly DefaultHttpContext fallbackContext = new DefaultHttpContext();
        private readonly IExceptionLogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public StackdriverExceptionHandler(IExceptionLogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;

            this.httpContextAccessor = httpContextAccessor;
        }

        public void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception exception)
        {
            if (exception != null && exception is not DomainException)
            {
                var httpContext = httpContextAccessor.HttpContext;

                logger.Log(exception, httpContext ?? fallbackContext);
            }
        }
    }
}
