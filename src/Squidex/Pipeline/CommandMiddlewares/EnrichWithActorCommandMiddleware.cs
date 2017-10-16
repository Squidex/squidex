// ==========================================================================
//  EnrichWithActorCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Write;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
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
            if (context.Command is SquidexCommand squidexCommand && squidexCommand.Actor == null)
            {
                var actorToken =
                    FindActorFromSubject() ??
                    FindActorFromClient();

                squidexCommand.Actor = actorToken ?? throw new SecurityException("No actor with subject or client id available.");
            }

            return next();
        }

        private RefToken FindActorFromSubject()
        {
            var subjectId = httpContextAccessor.HttpContext.User.OpenIdSubject();

            return subjectId == null ? null : new RefToken("subject", subjectId);
        }

        private RefToken FindActorFromClient()
        {
            var clientId = httpContextAccessor.HttpContext.User.OpenIdClientId();

            return clientId == null ? null : new RefToken("client", clientId);
        }
    }
}
