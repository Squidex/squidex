// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class RequestExceptionMiddleware : IMiddleware
    {
        private readonly ISemanticLog log;

        public RequestExceptionMiddleware(ISemanticLog log)
        {
            Guard.NotNull(log);

            this.log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w.WriteProperty("messag", "An unexpected exception has occurred."));

                context.Response.StatusCode = 500;
            }
        }
    }
}
