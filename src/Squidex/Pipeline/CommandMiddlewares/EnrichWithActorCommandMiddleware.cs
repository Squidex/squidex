// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class EnrichWithActorCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithActorCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return next();
            }

            if (context.Command is SquidexCommand squidexCommand)
            {
                var user = httpContextAccessor.HttpContext.User;

                if (squidexCommand.Actor == null)
                {
                    var actorToken = user.Token();

                    squidexCommand.Actor = actorToken ?? throw new SecurityException("No actor with subject or client id available.");
                }

                if (squidexCommand.User == null)
                {
                    squidexCommand.User = httpContextAccessor.HttpContext.User;
                }
            }

            return next();
        }
    }
}
