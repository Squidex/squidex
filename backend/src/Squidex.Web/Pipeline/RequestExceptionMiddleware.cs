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
using Squidex.Infrastructure.Security;

namespace Squidex.Web.Pipeline
{
    public sealed class RequestExceptionMiddleware : IMiddleware
    {
        private readonly IExceptionHandler exceptionHandler;

        public RequestExceptionMiddleware(IExceptionHandler exceptionHandler)
        {
            Guard.NotNull(exceptionHandler);

            this.exceptionHandler = exceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                exceptionHandler.Handle(ex);

                context.Response.StatusCode = 500;
            }
        }
    }
}
