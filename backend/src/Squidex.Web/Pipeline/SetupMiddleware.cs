// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Users;

namespace Squidex.Web.Pipeline
{
    public sealed class SetupMiddleware
    {
        private readonly RequestDelegate next;
        private bool isUserFound;

        public SetupMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            if (!isUserFound && await userService.IsEmptyAsync())
            {
                context.Response.Redirect("/identity-server/setup");
            }
            else
            {
                isUserFound = true;

                await next(context);
            }
        }
    }
}
