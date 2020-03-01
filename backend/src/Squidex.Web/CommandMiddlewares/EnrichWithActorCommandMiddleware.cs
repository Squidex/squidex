﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithActorCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithActorCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor);

            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return next(context);
            }

            if (context.Command is SquidexCommand squidexCommand)
            {
                var user = httpContextAccessor.HttpContext.User;

                if (squidexCommand.Actor == null)
                {
                    var actorToken = user.Token();

                    squidexCommand.Actor = actorToken ?? throw new DomainForbiddenException("No actor with subject or client id available.");
                }

                if (squidexCommand.User == null)
                {
                    squidexCommand.User = httpContextAccessor.HttpContext.User;
                }
            }

            return next(context);
        }
    }
}
