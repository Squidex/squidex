// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Squidex.Web;

namespace Squidex.Areas.Api.Config
{
    public sealed class IdentityServerPathMiddleware
    {
        private readonly RequestDelegate next;

        public IdentityServerPathMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.SetIdentityServerBasePath(Constants.IdentityServerPrefix);

            return next(context);
        }
    }
}
